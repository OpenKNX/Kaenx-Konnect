﻿using Kaenx.Konnect.Addresses;
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
        public event ConnectionChangedHandler ConnectionChanged;

        public bool IsConnected { get; set; }
        public ConnectionErrors LastError { get; set; }
        public UnicastAddress PhysicalAddress { get; set; }
        public int MaxFrameLength { get; set; } = 254;

        private ProtocolTypes CurrentType { get; set; } = ProtocolTypes.cEmi;
        private byte _sequenceCounter = 0;

        private readonly IPEndPoint _sendEndPoint;
        private List<UdpConnection> _clients = new List<UdpConnection>();
        private readonly Queue<object> _sendMessages;
        private readonly ReceiverParserDispatcher _receiveParserDispatcher;
        private CancellationTokenSource tokenSource;

        public KnxIpRouting(UnicastAddress physicalAddress, string ip = "224.0.23.12", int port = 3671)
        {
            _receiveParserDispatcher = new ReceiverParserDispatcher();
            _sendMessages = new Queue<object>();
            PhysicalAddress = physicalAddress;
            _sendEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            Init(ip, port);
        }

        private void Init(string ip, int port)
        {
// https://stackoverflow.com/questions/61661301/c-sharp-receive-multicast-udp-in-multiple-programs-on-the-same-machine
// https://www.winsocketdotnetworkprogramming.com/clientserversocketnetworkcommunication8l.html
// https://github.com/ChrisTTian667/knx-dotnet/blob/main/Knx/KnxNetIp/KnxNetIpRoutingClient.cs#L82
// https://github.com/lifeemotions/knx.net/blob/master/src/KNXLib/KnxConnectionRouting.cs#L75

            // UdpClient _udpClient = new UdpClient
            // {
            //     MulticastLoopback = false,
            //     ExclusiveAddressUse = false
            // };
            // _udpClient.Client.MulticastLoopback = false;

            // _udpClient.Client.SetSocketOption(
            //     SocketOptionLevel.Socket,
            //     SocketOptionName.ReuseAddress,
            //     true);

            // _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 3671));
            // _udpClient.JoinMulticastGroup(IPAddress.Parse("224.0.23.12"), IPAddress.Any);
            // _udpList.Add(_udpClient);

            // IEnumerable<IPAddress> ipv4Addresses =
            //         Dns
            //             .GetHostAddresses(Dns.GetHostName())
            //             .Where(i => i.AddressFamily == AddressFamily.InterNetwork);

            // foreach (IPAddress localIp in ipv4Addresses)
            // {
            //     var client = new UdpClient(new IPEndPoint(localIp, 3671));
            //     client.Client.MulticastLoopback = false;
            //     client.MulticastLoopback = false;
            //     _udpList.Add(client);
            //     client.JoinMulticastGroup(IPAddress.Parse("224.0.23.12"), localIp);
            // }

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
                    
                    IPAddress localIp = ipprops.UnicastAddresses.Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork).Single().Address;
                    
                    UdpConnection udp = new UdpConnection(localIp, port, new IPEndPoint(IPAddress.Parse(ip), port));
                    _clients.Add(udp);

                    Debug.WriteLine("Binded to " + adapter.Name + " - " + localIp.ToString() + " - " + 3671 + " -> " + udp.InterfaceIndex);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception binding to " + adapter.Name);
                    Debug.WriteLine(ex.Message);
                }
            }

            IsConnected = true;
            ConnectionChanged?.Invoke(true);
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

            _sendMessages.Enqueue(xdata.ToArray());

            return Task.CompletedTask;
        }

        public Task Send(byte[] data, bool ignoreConnected = false)
        {
            _sendMessages.Enqueue(data);
            return Task.CompletedTask;
        }

        public Task<byte> Send(IMessage message, bool ignoreConnected = false)
        {
            byte seq = _sequenceCounter++;
            message.SequenceCounter = seq;
            _sendMessages.Enqueue(message);

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
            foreach(UdpConnection conn in _clients)
                conn.OnReceived += KnxMessageReceived;

            tokenSource = new CancellationTokenSource();
            Task.Run(ProcessSendMessages, tokenSource.Token);

            return Task.CompletedTask;
        }

        public Task Disconnect()
        {
            //Nothing to do here
            foreach(UdpConnection conn in _clients)
                conn.OnReceived -= KnxMessageReceived;

            tokenSource.Cancel();

            return Task.CompletedTask;
        }

        public Task<bool> SendStatusReq()
        {
            return Task.FromResult<bool>(true);
        }

        private void KnxMessageReceived(UdpConnection sender, IParserMessage parserMessage)
        {
            try
            {
                switch (parserMessage)
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
                        Debug.WriteLine($"Received: {sender.Interface.Name} {tunnelResponse.APCI}");

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
                            OnTunnelRequest?.Invoke(msg);
                            break;
                        }

                    case SearchResponse searchResponse:
                        {
                            MsgSearchRes msg = new MsgSearchRes(searchResponse.responseBytes);
                            msg.ParseDataCemi();
                            OnTunnelResponse?.Invoke(msg);
                            break;
                        }

                    default:
                        throw new Exception("Not handled Telegram Type: " + parserMessage.GetType().ToString());
                }
            } catch (Exception ex)
            {
                Debug.WriteLine("Exception ProcessSendMessage: " + ex.Message);
            }
        }

        private void ProcessSendMessages()
        {
            while(!tokenSource.IsCancellationRequested)
            {
                if(_sendMessages.Count == 0)
                    continue;
                
                var sendMessage = _sendMessages.Dequeue();
                
                if (sendMessage is byte[])
                {
                    Debug.WriteLine("Sending bytes");
                    byte[] data = sendMessage as byte[];
                    foreach (UdpConnection client in _clients)
                        _ = client.SendAsync(data);
                }
                else if (sendMessage is MsgSearchReq)
                {
                    MsgSearchReq message = sendMessage as MsgSearchReq;

                    foreach(UdpConnection _udp in _clients)
                    {
                        message.Endpoint = _udp.GetLocalEndpoint();
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

                        _ = _udp.SendAsync(xdata);
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

                    foreach (UdpConnection client in _clients)
                        _ = client.SendAsync(xdata.ToArray());
                }
                else
                {
                    throw new Exception("Unbekanntes Element in SendQueue! " + sendMessage.GetType().FullName);
                }
            }
        }

        public void Dispose()
        {
            Disconnect();

            foreach (UdpConnection client in _clients)
                client.Dispose();
        }
    }
}