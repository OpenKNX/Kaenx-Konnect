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
    public class KnxIpTunneling : IKnxConnection
    {
        public event TunnelRequestHandler OnTunnelRequest;
        public event TunnelResponseHandler OnTunnelResponse;
        public event TunnelAckHandler OnTunnelAck;
        public event ConnectionChangedHandler ConnectionChanged;

        public bool IsConnected { get; set; }
        public ConnectionErrors LastError { get; set; }
        public UnicastAddress PhysicalAddress { get; set; }
        public int MaxFrameLength { get; set; } = 15;

        private ProtocolTypes CurrentType { get; set; } = ProtocolTypes.cEmi;
        private byte _communicationChannel;
        private byte _sequenceCounter = 0;

        private readonly IPEndPoint _receiveEndPoint;
        private readonly IPEndPoint _sendEndPoint;
        private UdpConnection _client;
        private readonly Queue<object> _sendMessages;
        
        private bool _flagCRRecieved = false;
        private List<int> _receivedAcks;
        private CancellationTokenSource _ackToken = null;
        private CancellationTokenSource tokenSource;

        private System.Timers.Timer _timer = new System.Timers.Timer(60000);
        private bool isInConfig = false;

        public KnxIpTunneling(string ip, int port)
        {
            _sendEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            IPAddress IP = GetIpAddress(ip);

            if (IP == null)
                throw new Exception("Lokale Ip konnte nicht gefunden werden");

            _receiveEndPoint = new IPEndPoint(IP, 0);
            _sendMessages = new Queue<object>();
            _receivedAcks = new List<int>();

            Init();
            _timer.Elapsed += TimerElapsed;
        }

        public KnxIpTunneling(IPEndPoint sendEndPoint)
        {
            _sendEndPoint = sendEndPoint;
            IPAddress ip = GetIpAddress(sendEndPoint.Address.ToString());

            if (ip == null)
                throw new Exception("Lokale Ip konnte nicht gefunden werden");

            _receiveEndPoint = new IPEndPoint(ip, 0);
            _sendMessages = new Queue<object>();

            Init();
            _timer.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _ = SendStatusReq();
        }

        private IPAddress GetIpAddress(string receiver)
        {
            if (receiver == "127.0.0.1")
                return IPAddress.Parse(receiver);

            IPAddress IP = null;
            int mostipcount = 0;
            string[] ipParts = receiver.Split('.');

            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

            foreach(NetworkInterface adapter in adapters)
            {
                IPInterfaceProperties properties = adapter.GetIPProperties();
                foreach(UnicastIPAddressInformation addr in properties.UnicastAddresses)
                {
                    int sameCount = 0;
                    string[] hostParts = addr.Address.ToString().Split('.');
                    for (int i = 0; i < 4; i++)
                    {
                        if (ipParts[i] != hostParts[i])
                        {
                            if (sameCount > mostipcount)
                            {
                                IP = addr.Address;
                                mostipcount = sameCount;
                            }
                            break;
                        }
                        sameCount++;
                    }
                    if (sameCount > mostipcount)
                    {
                        IP = addr.Address;
                        mostipcount = sameCount;
                    }
                }
            }
            
            if (IP == null)
            {
                try
                {
                    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                    {
                        socket.Connect("8.8.8.8", 65530);
                        IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                        IP = endPoint.Address;
                    }
                }
                catch { }
            }

            return IP;
        }

        private void Init()
        {
            _client = new UdpConnection(_receiveEndPoint.Address, _receiveEndPoint.Port, _sendEndPoint);
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

            _sendMessages.Enqueue(xdata.ToArray());

            return Task.CompletedTask;
        }

        public Task Send(byte[] data, bool ignoreConnected = false)
        {
            if (!ignoreConnected && !IsConnected)
                throw new Exception("Not connected with interface");

            _sendMessages.Enqueue(data);

            return Task.CompletedTask;
        }

        public Task<byte> Send(IMessage message, bool ignoreConnected = false)
        {
            if (!ignoreConnected && !IsConnected)
                throw new Exception("Not connected with interface");

            byte seq = _sequenceCounter++;
            message.SequenceCounter = seq;
            _sendMessages.Enqueue(message);

            return Task.FromResult(seq);
        }

        public async Task Connect()
        {
            await Connect(false);
        }

        public async Task Connect(bool connectOnly = false)
        {
            _client.OnReceived += KnxMessageReceived;
            _flagCRRecieved = false;
            ConnectionRequest builder = new ConnectionRequest();
            builder.Build(_receiveEndPoint);
            _ackToken = new CancellationTokenSource();
            await _client.SendAsync(builder.GetBytes());
            try{
                await Task.Delay(500, _ackToken.Token);
            }catch{}

            if (!_flagCRRecieved)
            {
                throw new Exception("Schnittstelle ist nicht erreichbar!");
            }

            if (!IsConnected)
            {
                throw new Exception("Verbindung zur Schnittstelle konnte nicht hergestellt werden! Error: " + LastError);
            }
            
            // bool state = await SendStatusReq();
            // if (!state)
            // {
            //     throw new Exception("Die Schnittstelle hat keine Verbindung zum Bus! Error: " + LastError);
            // }
            
            if(!connectOnly)
            {
                _client.OnReceived -= KnxMessageReceived;

                KnxIpTunnelingConfig conf = new KnxIpTunnelingConfig(_client, _receiveEndPoint);

                try{
                    isInConfig = true;
                    await conf.Connect();
                } catch {
                    //do nothing?
                } finally {
                    await conf.Disconnect();
                    isInConfig = false;
                }
                
                _client.OnReceived += KnxMessageReceived;
            }

            tokenSource = new CancellationTokenSource();
            _ = Task.Run(ProcessSendMessages, tokenSource.Token);

            _timer.Start();
        }

        public Task Disconnect()
        {
            _client.OnReceived -= KnxMessageReceived;
            if (!IsConnected)
                return Task.CompletedTask;

            DisconnectRequest builder = new DisconnectRequest();
            builder.Build(_receiveEndPoint, _communicationChannel);
            Send(builder.GetBytes(), true);

            tokenSource.Cancel();

            if(isInConfig)
            {
                // Really disconnect?
            }

            _timer.Stop();
            return Task.CompletedTask;
        }

        public async Task<bool> SendStatusReq()
        {
            ConnectionStatusRequest stat = new ConnectionStatusRequest();
            stat.Build(_receiveEndPoint, _communicationChannel);
            stat.SetChannelId(_communicationChannel);
            await Send(stat.GetBytes());
            await Task.Delay(200);
            return IsConnected;
        }

        private void KnxMessageReceived(UdpConnection sender, IParserMessage parserMessage)
        {
            try
            {
                switch (parserMessage)
                {
                    case ConnectStateResponse connectStateResponse:
                        if(connectStateResponse.CommunicationChannel != _communicationChannel) return;
                        //Debug.WriteLine("Connection State Response: " + connectStateResponse.Status.ToString());
                        switch (connectStateResponse.Status)
                        {
                            case 0x00:
                                IsConnected = true;
                                ConnectionChanged?.Invoke(IsConnected);
                                break;
                            default:
                                //Debug.WriteLine("Connection State: Fehler: " + connectStateResponse.Status.ToString());
                                LastError = ConnectionErrors.NotConnectedToBus;
                                IsConnected = false;
                                ConnectionChanged?.Invoke(IsConnected);
                                break;
                        }
                        break;

                    case ConnectResponse connectResponse:
                        _flagCRRecieved = true;
                        switch (connectResponse.Status)
                        {
                            case 0x00:
                                _sequenceCounter = 0;
                                _communicationChannel = connectResponse.CommunicationChannel;
                                IsConnected = true;
                                ConnectionChanged?.Invoke(IsConnected);
                                PhysicalAddress = connectResponse.ConnectionResponseDataBlock.KnxAddress;
                                //Debug.WriteLine("Connected: Eigene Adresse: " + PhysicalAddress.ToString());
                                break;
                            default:
                                //Debug.WriteLine("Connected: Fehler: " + connectResponse.Status.ToString());
                                LastError = ConnectionErrors.Undefined;
                                IsConnected = false;
                                ConnectionChanged?.Invoke(IsConnected);
                                break;
                        }
                        //if(_ackToken != null)
                        //    _ackToken.Cancel();
                        break;

                    case Requests.TunnelRequest tunnelResponse:
                        if(tunnelResponse.CommunicationChannel != _communicationChannel) return;
                        if (tunnelResponse.APCI.ToString().EndsWith("Request") && tunnelResponse.DestinationAddress != PhysicalAddress)
                        {
                            //Debug.WriteLine("Telegram erhalten das nicht mit der Adresse selbst zu tun hat!");
                            //Debug.WriteLine("Typ: " + tunnelResponse.APCI);
                            //Debug.WriteLine("Eigene Adresse: " + PhysicalAddress.ToString());
                            break;
                        }

                        _sendMessages.Enqueue(new Responses.TunnelResponse(0x06, 0x10, 0x0A, 0x04, _communicationChannel, tunnelResponse.SequenceCounter, 0x00).GetBytes());

                        //Debug.WriteLine("Telegram APCI: " + tunnelResponse.APCI.ToString());

                        if (tunnelResponse.IsNumbered && tunnelResponse.APCI.ToString().EndsWith("Response"))
                        {
                            Messages.Response.MsgAckRes msgAckRes = new MsgAckRes
                            {
                                DestinationAddress = tunnelResponse.SourceAddress,
                                SequenceNumber = tunnelResponse.SequenceNumber
                            };
                            _ = Send(msgAckRes);
                        }
                        else if (tunnelResponse.APCI == ApciTypes.Ack)
                        {
                            OnTunnelAck?.Invoke(new MsgAckRes()
                            {
                                ChannelId = tunnelResponse.CommunicationChannel,
                                SequenceCounter = tunnelResponse.SequenceCounter,
                                SequenceNumber = tunnelResponse.SequenceNumber,
                                SourceAddress = tunnelResponse.SourceAddress,
                                DestinationAddress = tunnelResponse.DestinationAddress
                            });
                            break;
                        }

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
                            if (tunnelResponse.APCI.ToString().EndsWith("Response"))
                            {
                                message = new MsgDefaultRes(tunnelResponse.IsNumbered)
                                {
                                    ApciType = tunnelResponse.APCI
                                };
                            } else {
                                message = new MsgDefaultReq(tunnelResponse.IsNumbered)
                                {
                                    ApciType = tunnelResponse.APCI
                                };
                            }
                            //Debug.WriteLine("Kein MessageParser für den APCI " + tunnelResponse.APCI);
                        }

                        message.Raw = tunnelResponse.Data;
                        message.ChannelId = tunnelResponse.CommunicationChannel;
                        message.SequenceCounter = tunnelResponse.SequenceCounter;
                        message.SequenceNumber = tunnelResponse.SequenceNumber;
                        message.SourceAddress = tunnelResponse.SourceAddress;
                        message.DestinationAddress = tunnelResponse.DestinationAddress;

                        switch (CurrentType)
                        {
                            case ProtocolTypes.cEmi:
                                message.ParseDataCemi();
                                break;
                            case ProtocolTypes.Emi1:
                                message.ParseDataEmi1();
                                break;
                            case ProtocolTypes.Emi2:
                                message.ParseDataEmi2();
                                break;
                            default:
                                throw new NotImplementedException("Unbekanntes Protokoll - TunnelResponse KnxIpTunneling");
                        }


                        if (tunnelResponse.APCI.ToString().EndsWith("Response"))
                            OnTunnelResponse?.Invoke(message as IMessageResponse);
                        else
                            OnTunnelRequest?.Invoke(message as IMessageRequest);

                        break;

                    case Requests.SearchRequest searchRequest:
                    {
                        MsgSearchReq msg = new MsgSearchReq(searchRequest.responseBytes);
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
                        OnTunnelRequest?.Invoke(msg);
                        break;
                    }

                    case SearchResponse searchResponse:
                    {
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
                        OnTunnelResponse?.Invoke(msg);
                        break;
                    }

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
            while (!tokenSource.IsCancellationRequested)
            {
                if(_sendMessages.Count == 0)
                    continue;
                
                var sendMessage = _sendMessages.Dequeue();
                
                if (sendMessage is byte[])
                {

                    byte[] data = sendMessage as byte[];
                    await _client.SendAsync(data);
                }
                else if (sendMessage is MsgSearchReq || sendMessage is MsgSearchRes)
                {
                    IMessage message = (IMessage)sendMessage;
                    if(message is MsgSearchReq msr)
                        msr.Endpoint = _receiveEndPoint;

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

                    await _client.SendAsync(xdata);
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
                        // if(repeatCounter > 0)
                        // {
                        //     Console.WriteLine("wiederhole telegrmm " + message.SequenceCounter.ToString());
                        // }
                        if(repeatCounter > 3)
                            throw new Exception("Zu viele wiederholungen eines Telegramms auf kein OK");

                        await _client.SendAsync(xdata.ToArray());
                        
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

        public void Dispose()
        {
            if(_client != null)
                _client.Dispose();
            tokenSource.Cancel();
        }
    }
}