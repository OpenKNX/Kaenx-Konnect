using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Messages;
using Kaenx.Konnect.Messages.Request;
using Kaenx.Konnect.Messages.Response;
using Kaenx.Konnect.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using static Kaenx.Konnect.Connections.IKnxConnection;

namespace Kaenx.Konnect.Connections
{
    public class KnxIpSearch : IKnxConnection
    {
        public delegate void SearchResponseHandler(MsgSearchRes message, NetworkInterface netInterface, int netIndex);
        public delegate void SearchRequestHandler(MsgSearchReq message);

        public event TunnelRequestHandler OnTunnelRequest;
        public event TunnelResponseHandler OnTunnelResponse;
        public event TunnelAckHandler OnTunnelAck;
        public event SearchResponseHandler OnSearchResponse;
        public event SearchRequestHandler OnSearchRequest;
        public event ConnectionChangedHandler ConnectionChanged;

        public bool IsConnected { get; set; }
        public ConnectionErrors LastError { get; set; }
        public UnicastAddress PhysicalAddress { get; set; }
        public int MaxFrameLength { get; set; } = 15;

        private ProtocolTypes CurrentType { get; set; } = ProtocolTypes.cEmi;
        private byte _communicationChannel;
        private bool StopProcessing = false;
        private byte _sequenceCounter = 0;

        private readonly IPEndPoint _sendEndPoint;
        private List<UdpConnection> _clients = new List<UdpConnection>();
        private readonly BlockingCollection<object> _sendMessages;
        
        private List<int> _receivedAcks;
        private CancellationTokenSource _ackToken = null;
        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        public KnxIpSearch()
        {
            _sendEndPoint = new IPEndPoint(IPAddress.Parse("224.0.23.12"), 3671);

            _sendMessages = new BlockingCollection<object>();
            _receivedAcks = new List<int>();

            Init();

            if(OnTunnelResponse != null && OnTunnelRequest != null && OnTunnelAck != null && OnSearchRequest != null)
                return;
        }

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _ = SendStatusReq();
        }

        private void Init()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
           
            foreach (NetworkInterface adapter in nics)
            {
                try
                {
                    IPInterfaceProperties ipprops = adapter.GetIPProperties();
                    if (ipprops.MulticastAddresses.Count == 0 // most of VPN adapters will be skipped
                        || !adapter.SupportsMulticast // multicast is meaningless for this type of connection
                        || OperationalStatus.Up != adapter.OperationalStatus) // this adapter is off or not connected
                    {
                        Debug.WriteLine("Skipped " + adapter.Name + " maybe vpn or off");
                        continue;
                    }
                        
                    IPv4InterfaceProperties p = ipprops.GetIPv4Properties();
                    if (null == p) // IPv4 is not configured on this adapter
                    {
                        Debug.WriteLine("Skipped " + adapter.Name + " ip4 not configured");
                        continue;
                    }
                    
                    IPAddress addr = ipprops.UnicastAddresses.Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork).Single().Address;
                    
                    UdpConnection udp = new UdpConnection(addr, _sendEndPoint); //TODO set source?
                    udp.InterfaceIndex = IPAddress.HostToNetworkOrder(p.Index);
                    udp.Interface = adapter;
                    _clients.Add(udp);

                    Debug.WriteLine("Binded to " + adapter.Name + " - " + addr.ToString() + " - " + udp.GetLocalEndpoint().Port + " -> " + udp.InterfaceIndex);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception binding to " + adapter.Name);
                    Debug.WriteLine(ex.Message);
                }
            }
            
            Task.Run(ProcessSendMessages, tokenSource.Token);
        }

        public Task Send(byte[] data, byte sequence)
        {
            List<byte> xdata = new List<byte>();

            //KNX/IP Header
            xdata.Add(0x06); //Header Length
            xdata.Add(0x10); //Protokoll Version 1.0
            xdata.Add(0x04); //Service Identifier Family: Tunneling
            xdata.Add(0x20); //Service Identifier Type: Request
            xdata.AddRange(BitConverter.GetBytes(Convert.ToInt16(data.Length + 10)).Reverse()); //Total length. Set later

            //Connection header
            xdata.Add(0x04); // Body Structure Length
            xdata.Add(_communicationChannel); // Channel Id
            xdata.Add(sequence); // Sequenz Counter
            xdata.Add(0x00); // Reserved
            xdata.AddRange(data);

            _sendMessages.Add(xdata.ToArray());

            return Task.CompletedTask;
        }

        public Task Send(byte[] data, bool ignoreConnected = false)
        {
            if (!ignoreConnected && !IsConnected)
                throw new Exception("Not connected with interface");

            _sendMessages.Add(data);

            return Task.CompletedTask;
        }

        public Task<byte> Send(IMessage message, bool ignoreConnected = false)
        {
            if (!ignoreConnected && !IsConnected)
                throw new Exception("Not connected with interface");

            byte seq = _sequenceCounter++;
            message.SequenceCounter = seq;
            _sendMessages.Add(message);

            return Task.FromResult(seq);
        }

        public async Task Connect()
        {
            await Connect(false);
        }

        public async Task Connect(bool connectOnly = false)
        {
            foreach(UdpConnection udp in _clients)
                udp.OnReceived += KnxMessageReceived;
        }

        public async Task Disconnect()
        {
            foreach(UdpConnection udp in _clients)
                udp.OnReceived -= KnxMessageReceived;
        }

        public async Task<bool> SendStatusReq()
        {
            return true;
        }

        private void KnxMessageReceived(UdpConnection sender, IParserMessage parserMessage)
        {
            try
            {
                switch (parserMessage)
                {
                    case SearchResponse searchResponse:
                        MsgSearchRes msg = new MsgSearchRes(searchResponse.responseBytes);
                        switch (CurrentType)
                        {
                            case ProtocolTypes.cEmi:
                                msg.ParseDataCemi();
                                break;
                            case ProtocolTypes.Emi1:
                                msg.ParseDataEmi1();
                                break;
                            case ProtocolTypes.Emi2:
                                msg.ParseDataEmi2();
                                break;
                            default:
                                throw new NotImplementedException("Unbekanntes Protokoll - SearchResponse KnxIpTunneling");
                        }
                        OnSearchResponse?.Invoke(msg, sender.Interface, sender.InterfaceIndex);
                        break;

                    case TunnelAckResponse tunnelAck:
                        if(tunnelAck.ChannelId != _communicationChannel) return;
                        _receivedAcks.Add(tunnelAck.SequenceCounter);
                        if(_ackToken != null)
                            _ackToken.Cancel();
                        break;

                    case Kaenx.Konnect.Requests.DisconnectRequest disconnectRequest:
                    {
                        if(disconnectRequest.CommunicationChannel != _communicationChannel) return;
                        IsConnected = false;
                        _communicationChannel = 0;
                        ConnectionChanged?.Invoke(IsConnected);
                        Debug.WriteLine("Die Verbindung wurde vom Gerät geschlossen");
                        break;
                    }

                    case DisconnectResponse disconnectResponse:
                        if(disconnectResponse.CommunicationChannel != _communicationChannel) return;
                        IsConnected = false;
                        _communicationChannel = 0;
                        ConnectionChanged?.Invoke(IsConnected);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception ProcessSendMessage: " + ex.Message);
            }
        }

        private async Task ProcessSendMessages()
        {
            while (!StopProcessing)
            {
                foreach (var sendMessage in _sendMessages.GetConsumingEnumerable())
                {
                    if (sendMessage is byte[])
                    {

                        byte[] data = sendMessage as byte[];
                        foreach(UdpConnection udp in _clients)
                            await udp.SendAsync(data);
                    }
                    else if (sendMessage is MsgSearchReq || sendMessage is MsgSearchRes)
                    {
                        IMessage message = (IMessage)sendMessage;

                        
                        foreach(UdpConnection udp in _clients)
                        {
                            if(message is MsgSearchReq msr)
                                msr.Endpoint = udp.GetLocalEndpoint();

                            byte[] xdata;

                            switch (CurrentType)
                            {
                                case ProtocolTypes.Emi1:
                                    xdata = message.GetBytesEmi1();
                                    break;

                                case ProtocolTypes.Emi2:
                                    xdata = message.GetBytesEmi1(); //Todo check diffrences to emi1
                                    //xdata.AddRange(message.GetBytesEmi2());
                                    break;

                                case ProtocolTypes.cEmi:
                                    xdata = message.GetBytesCemi();
                                    break;

                                default:
                                    throw new Exception("Unbekanntes Protokoll");
                            }
                            await udp.SendAsync(xdata);
                        }
                    }
                    else if (sendMessage is IMessage)
                    {
                        IMessage message = sendMessage as IMessage;
                        message.SourceAddress = UnicastAddress.FromString("0.0.0");
                        List<byte> xdata = new List<byte>
                        {
                            //KNX/IP Header
                            0x06, //Header Length
                            0x10, //Protokoll Version 1.0
                            0x04, //Service Identifier Family: Tunneling
                            0x20, //Service Identifier Type: Request
                            0x00, //Total length. Set later
                            0x00, //Total length. Set later
                            
                            //Connection header
                            0x04, // Body Structure Length
                            _communicationChannel, // Channel Id
                            message.SequenceCounter, // Sequenz Counter
                            0x00  //Reserved
                        };

                        if(_receivedAcks.Contains(message.SequenceCounter))
                            _receivedAcks.Remove(message.SequenceCounter);

                        switch (CurrentType)
                        {
                            case ProtocolTypes.Emi1:
                                xdata.AddRange(message.GetBytesEmi1());
                                break;

                            case ProtocolTypes.Emi2:
                                xdata.AddRange(message.GetBytesEmi1()); //Todo check diffrences between emi1
                                                                        //xdata.AddRange(message.GetBytesEmi2());
                                break;

                            case ProtocolTypes.cEmi:
                                xdata.AddRange(message.GetBytesCemi());
                                break;

                            default:
                                throw new Exception("Unbekanntes Protokoll");
                        }

                        byte[] length = BitConverter.GetBytes((ushort)xdata.Count);
                        Array.Reverse(length);
                        xdata[4] = length[0];
                        xdata[5] = length[1];

                        int repeatCounter = 0;
                        do 
                        {
                            if(repeatCounter > 0)
                            {
                                Console.WriteLine("wiederhole telegrmm " + message.SequenceCounter.ToString());
                            }
                            if(repeatCounter > 3)
                                throw new Exception("Zu viele wiederholungen eines Telegramms auf kein OK");

                            foreach(UdpConnection udp in _clients)
                                await udp.SendAsync(xdata.ToArray());
                            
                            _ackToken = new CancellationTokenSource();
                            
                            try{
                                await Task.Delay(1000, _ackToken.Token);
                            }catch{}
                            _ackToken = null;

                            repeatCounter++;
                        } while(!_receivedAcks.Contains(message.SequenceCounter));
                    }
                    else
                    {
                        throw new Exception("Unbekanntes Element in SendQueue! " + sendMessage.GetType().FullName);
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach(UdpConnection udp in _clients)
                udp.Dispose();
            tokenSource.Cancel();
        }
    }
}