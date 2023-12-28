using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Messages;
using Kaenx.Konnect.Messages.Request;
using Kaenx.Konnect.Messages.Response;
using Kaenx.Konnect.Parser;
using Kaenx.Konnect.Requests;
using Kaenx.Konnect.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Kaenx.Konnect.Connections.IKnxConnection;

namespace Kaenx.Konnect.Connections
{
    public class KnxIpRouting : IKnxConnection
    {
        public event TunnelRequestHandler OnTunnelRequest;
        public event TunnelResponseHandler OnTunnelResponse;
        public event TunnelAckHandler OnTunnelAck;
        public event SearchResponseHandler OnSearchResponse;
        public event SearchRequestHandler OnSearchRequest;
        public event ConnectionChangedHandler ConnectionChanged;

        public bool IsConnected { get; set; }
        public ConnectionErrors LastError { get; set; }
        public UnicastAddress PhysicalAddress { get; set; }

        private ProtocolTypes CurrentType { get; set; } = ProtocolTypes.cEmi;
        private bool StopProcessing = false;
        private byte _sequenceCounter = 0;

        private readonly IPEndPoint _sendEndPoint;
        private List<UdpClient> _udpList = new List<UdpClient>();
        private readonly BlockingCollection<object> _sendMessages;
        private readonly ReceiverParserDispatcher _receiveParserDispatcher;

/*
        public KnxIpRouting(string ip = "224.0.23.12", int port = 3671)
        {
            _receiveParserDispatcher = new ReceiverParserDispatcher();
            _sendMessages = new BlockingCollection<object>();

            Init(ip, port);
        }
*/

        public KnxIpRouting(UnicastAddress physicalAddress, string ip = "224.0.23.12", int port = 3671)
        {
            _receiveParserDispatcher = new ReceiverParserDispatcher();
            _sendMessages = new BlockingCollection<object>();
            PhysicalAddress = physicalAddress;
            _sendEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            Init(ip, port);
        }

        private void Init(string ip, int port)
        {
            /*UdpClient client = new UdpClient();

            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            //client.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, 100663296);
            client.JoinMulticastGroup(IPAddress.Parse(ip), IPAddress.Any);
            client.MulticastLoopback = true;
            client.Client.MulticastLoopback = true;
            _udpList.Add(client);

            ProcessSendMessages();
            ProcessReceivingMessages(client);
            */

            
            
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
           
            foreach (NetworkInterface adapter in nics)
            {

                try
                {
                    IPInterfaceProperties ipprops = adapter.GetIPProperties();
                    if (ipprops.MulticastAddresses.Count == 0 // most of VPN adapters will be skipped
                        || !adapter.SupportsMulticast // multicast is meaningless for this type of connection
                        || OperationalStatus.Up != adapter.OperationalStatus) // this adapter is off or not connected
                        continue;
                    IPv4InterfaceProperties p = ipprops.GetIPv4Properties();
                    if (null == p) continue; // IPv4 is not configured on this adapter
                    int index = IPAddress.HostToNetworkOrder(p.Index);

                    IPAddress addr = adapter.GetIPProperties().UnicastAddresses.Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork).Single().Address;
                    UdpClient _udpClient = new UdpClient();
                    _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    _udpClient.Client.Bind(new IPEndPoint(addr, 3671));
                    _udpClient.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, index);
                    _udpClient.JoinMulticastGroup(IPAddress.Parse("224.0.23.12"), IPAddress.Any);
                    _udpClient.MulticastLoopback = true;
                    _udpClient.Client.MulticastLoopback = true;
                    _udpList.Add(_udpClient);

                    Debug.WriteLine("Binded to " + adapter.Name + " - " + addr.ToString() + " - 3671 -> " + index);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception binding to " + adapter.Name);
                    Debug.WriteLine(ex.Message);
                }
            }


            ProcessSendMessages();
            

            foreach (UdpClient client in _udpList)
                ProcessReceivingMessages(client);

            IsConnected = true;
            ConnectionChanged?.Invoke(true);
        }

        public static int GetFreePort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }


        public Task Send(byte[] data, byte sequence)
        {
            List<byte> xdata = new List<byte>();

            //KNX/IP Header
            xdata.Add(0x06); //Header Length
            xdata.Add(0x10); //Protokoll Version 1.0
            xdata.Add(0x05); //Service Identifier Family: Tunneling
            xdata.Add(0x30); //Service Identifier Type: Request
            xdata.AddRange(BitConverter.GetBytes(Convert.ToInt16(data.Length + 6)).Reverse()); //Total length. Set later

            xdata.AddRange(data);

            _sendMessages.Add(xdata.ToArray());

            return Task.CompletedTask;
        }


        public Task Send(byte[] data, bool ignoreConnected = false)
        {
            _sendMessages.Add(data);
            return Task.CompletedTask;
        }

        public Task<byte> Send(IMessage message, bool ignoreConnected = false)
        {
            byte seq = _sequenceCounter++;
            message.SequenceCounter = seq;
            _sendMessages.Add(message);

            return Task.FromResult(seq);
        }

        public void Search() 
        {
            //Dont know if we can send this hear
            //Send(new MsgSearchReq());
        }

        public Task Connect()
        {
            //Nothing to do here
            return Task.CompletedTask;
        }

        public Task Disconnect()
        {
            //Nothing to do here
            return Task.CompletedTask;
        }

        public Task<bool> SendStatusReq()
        {
            return Task.FromResult<bool>(true);
        }


        private void ProcessReceivingMessages(UdpClient _udpClient)
        {
            Debug.WriteLine("Höre jetzt auf: " + (_udpClient.Client.LocalEndPoint as IPEndPoint)?.Port);
            Task.Run(async () =>
            {
                int rofl = 0;
                try
                {

                    while (!StopProcessing)
                    {
                        rofl++;
                        var result = await _udpClient.ReceiveAsync();
                        var knxResponse = _receiveParserDispatcher.Build(result.Buffer);
                        if(knxResponse == null) continue;

                        switch (knxResponse)
                        {
                            case Responses.RoutingResponse tunnelResponse:
                                if (tunnelResponse.DestinationAddress.ToString() != PhysicalAddress.ToString())
                                {
                                    Debug.WriteLine("Telegram erhalten das nicht mit der Adresse selbst zu tun hat!");
                                    Debug.WriteLine("Typ: " + tunnelResponse.APCI);
                                    Debug.WriteLine("Eigene Adresse: " + PhysicalAddress.ToString());
                                    Debug.WriteLine("Adressiert an:  " + tunnelResponse.DestinationAddress.ToString());
                                    break;
                                }

                                if (tunnelResponse.APCI.ToString().EndsWith("Response"))
                                {
                                    List<byte> data = new List<byte>() { 0x11, 0x00 };
                                    Builders.TunnelRequest builder = new Builders.TunnelRequest();
                                    builder.Build(PhysicalAddress, tunnelResponse.SourceAddress, ApciTypes.Ack, tunnelResponse.SequenceNumber);
                                    data.AddRange(builder.GetBytes());
                                    _=Send(data.ToArray(), _sequenceCounter);
                                    _sequenceCounter++;
                                    
                                }
                                else if (tunnelResponse.APCI == ApciTypes.Ack)
                                {
                                    OnTunnelAck?.Invoke(new MsgAckRes()
                                    {
                                        //ChannelId = tunnelResponse.CommunicationChannel,
                                        //SequenceCounter = tunnelResponse.SequenceCounter,
                                        SequenceNumber = tunnelResponse.SequenceNumber,
                                        SourceAddress = tunnelResponse.SourceAddress,
                                        DestinationAddress = tunnelResponse.DestinationAddress
                                    });
                                    break;
                                }


                                List<string> temp = new List<string>();
                                var q = from t in Assembly.GetExecutingAssembly().GetTypes()
                                        where t.IsClass && t.IsNested == false && (t.Namespace == "Kaenx.Konnect.Messages.Response" || t.Namespace == "Kaenx.Konnect.Messages.Request")
                                        select t;

                                IMessage message = null;

                                foreach (Type t in q.ToList())
                                {
                                    IMessage resp = (IMessage)Activator.CreateInstance(t);

                                    if (resp.ApciType == tunnelResponse.APCI)
                                    {
                                        message = resp;
                                        break;
                                    }
                                }


                                if (message == null)
                                {
                                    //throw new Exception("Kein MessageParser für den APCI " + tunnelResponse.APCI);
                                    message = new MsgDefaultRes()
                                    {
                                        ApciType = tunnelResponse.APCI
                                    };
                                    Debug.WriteLine("Kein MessageParser für den APCI " + tunnelResponse.APCI);
                                }

                                message.Raw = tunnelResponse.Data;
                                //message.ChannelId = tunnelResponse.CommunicationChannel;
                                //message.SequenceCounter = tunnelResponse.SequenceCounter;
                                message.SequenceNumber = tunnelResponse.SequenceNumber;
                                message.SourceAddress = tunnelResponse.SourceAddress;
                                message.DestinationAddress = tunnelResponse.DestinationAddress;

                                //routing is only allowed with cemi
                                message.ParseDataCemi();

                                if (tunnelResponse.APCI.ToString().EndsWith("Response"))
                                    OnTunnelResponse?.Invoke(message as IMessageResponse);
                                else
                                    OnTunnelRequest?.Invoke(message as IMessageRequest);

                                break;



                            case SearchRequest searchRequest:
                                {
                                    MsgSearchReq msg = new MsgSearchReq(searchRequest.responseBytes);
                                    msg.ParseDataCemi();
                                    OnSearchRequest?.Invoke(msg);
                                    break;
                                }

                            case SearchResponse searchResponse:
                                {
                                    MsgSearchRes msg = new MsgSearchRes(searchResponse.responseBytes);
                                    msg.ParseDataCemi();
                                    OnSearchResponse?.Invoke(msg);
                                    break;
                                }

                            default:
                                throw new Exception("Not handled Telegram Type: " + knxResponse.GetType().ToString());
                        }
                    }

                    Debug.WriteLine("Stopped Processing Messages " + _udpClient.Client.LocalEndPoint.ToString());
                    _udpClient.Close();
                    _udpClient.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Processing Messages Exception:" + ex.Message);
                }
            });
        }

        private void ProcessSendMessages()
        {
            Task.Run(() =>
            {

                foreach (var sendMessage in _sendMessages.GetConsumingEnumerable())
                {
                    if (sendMessage is byte[])
                    {
                        Debug.WriteLine("Sending bytes");
                        byte[] data = sendMessage as byte[];
                        foreach (UdpClient client in _udpList)
                            client.SendAsync(data, data.Length, _sendEndPoint);
                    }
                    else if (sendMessage is MsgSearchReq)
                    {
                        MsgSearchReq message = sendMessage as MsgSearchReq;

                        foreach(UdpClient _udp in _udpList)
                        {
                            message.Endpoint = _udp.Client.LocalEndPoint as IPEndPoint;
                            byte[] xdata;

                            switch (CurrentType)
                            {
                                case ProtocolTypes.Emi1:
                                    xdata = message.GetBytesEmi1();
                                    break;

                                case ProtocolTypes.Emi2:
                                    xdata = message.GetBytesEmi2(); //Todo check diffrences to emi1
                                                                    //xdata.AddRange(message.GetBytesEmi2());
                                    break;

                                case ProtocolTypes.cEmi:
                                    xdata = message.GetBytesCemi();
                                    break;

                                default:
                                    throw new Exception("Unbekanntes Protokoll");
                            }

                            _udp.SendAsync(xdata, xdata.Length, _sendEndPoint);
                        }
                    } else if(sendMessage is IMessage) { 
                        Debug.WriteLine("Sending IMessage " + sendMessage.GetType());
                        IMessage message = sendMessage as IMessage;
                        message.SourceAddress = PhysicalAddress;
                        List<byte> xdata = new List<byte>();

                        //KNX/IP Header
                        xdata.Add(0x06); //Header Length
                        xdata.Add(0x10); //Protokoll Version 1.0
                        xdata.Add(0x05); //Service Identifier Family: Tunneling
                        xdata.Add(0x30); //Service Identifier Type: Request
                        xdata.AddRange(new byte[] { 0x00, 0x00 }); //Total length. Set later

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

                        byte[] length = BitConverter.GetBytes((ushort)(xdata.Count));
                        Array.Reverse(length);
                        xdata[4] = length[0];
                        xdata[5] = length[1];


                        //_udp.SendAsync(xdata.ToArray(), xdata.Count, _sendEndPoint);

                        foreach (UdpClient client in _udpList)
                            client.SendAsync(xdata.ToArray(), xdata.Count, _sendEndPoint);
                    }
                    else
                    {
                        throw new Exception("Unbekanntes Element in SendQueue! " + sendMessage.GetType().FullName);
                    }
                }
            });
        }
    }
}
