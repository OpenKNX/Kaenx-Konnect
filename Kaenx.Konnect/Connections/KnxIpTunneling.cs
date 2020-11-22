﻿using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Messages;
using Kaenx.Konnect.Messages.Request;
using Kaenx.Konnect.Parser;
using Kaenx.Konnect.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Kaenx.Konnect.Connections.IKnxConnection;

namespace Kaenx.Konnect.Connections
{
    public class KnxIpTunneling : IKnxConnection
    {
        public event TunnelRequestHandler OnTunnelRequest;
        public event TunnelRequestHandler OnTunnelResponse;
        public event TunnelRequestHandler OnTunnelAck;
        public event SearchResponseHandler OnSearchResponse;
        public event ConnectionChangedHandler ConnectionChanged;


        public int Port;
        public bool IsConnected { get; set; }

        private ProtocolTypes CurrentType { get; set; } = ProtocolTypes.cEmi;
        private byte _communicationChannel;
        private bool StopProcessing = false;
        private byte _sequenceCounter = 0;
        private UnicastAddress SelfAddress;

        private readonly IPEndPoint _receiveEndPoint;
        private readonly IPEndPoint _sendEndPoint;
        private List<UdpClient> _udpList = new List<UdpClient>();
        private readonly BlockingCollection<object> _sendMessages;
        private readonly ReceiverParserDispatcher _receiveParserDispatcher;

        public KnxIpTunneling(IPEndPoint sendEndPoint, bool sendToAll = false)
        {
            Port = GetFreePort();
            _sendEndPoint = sendEndPoint;
            IPAddress ip = null;

            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                ip = endPoint.Address;
            }

            if (ip == null)
                throw new Exception("Lokale Ip konnte nicht gefunden werden");

            _receiveEndPoint = new IPEndPoint(ip, Port);
            _receiveParserDispatcher = new ReceiverParserDispatcher();
            _sendMessages = new BlockingCollection<object>();



            if (sendToAll)
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in nics)
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
                    UdpClient _udpClient = new UdpClient(new IPEndPoint(addr, GetFreePort()));
                    _udpClient.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, index);
                    _udpList.Add(_udpClient);

                    Debug.WriteLine("Binded to " + adapter.Name);
                }
            } else
            {
                UdpClient _udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, GetFreePort()));
                _udpList.Add(_udpClient);
                Debug.WriteLine("Binded to default");
            }

            

            ProcessSendMessages();
            try
            {
                foreach (UdpClient client in _udpList)
                    ProcessReceivingMessages(client);
            } catch
            {

            }
        }





        public static int GetFreePort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }



        public Task Send(byte[] data, bool ignoreConnected = false)
        {
            if (!ignoreConnected && !IsConnected)
                throw new Exception("Roflkopter 1");

            _sendMessages.Add(data);

            return Task.CompletedTask;
        }

        public Task<byte> Send(IMessageRequest message, bool ignoreConnected = false)
        {
            if (!ignoreConnected && !IsConnected)
                throw new Exception("Roflkopter 2");

            var seq = _sequenceCounter;
            message.SetInfo(_communicationChannel, _sequenceCounter);
            _sequenceCounter++;
            _sendMessages.Add(message);

            return Task.FromResult(seq);
        }




        public async Task Connect()
        {
            ConnectionRequest builder = new ConnectionRequest();
            builder.Build(_receiveEndPoint, 0x00);
            await Send(builder.GetBytes(), true);
            await Task.Delay(200);
            if (!IsConnected)
                throw new Exception("Verbindung zur Schnittstelle konnte nicht hergestellt werden!");
            await SendStatusReq();
            await Task.Delay(200);
        }

        public Task Disconnect()
        {
            if (!IsConnected)
                return Task.CompletedTask;

            DisconnectRequest builder = new DisconnectRequest();
            builder.Build(_receiveEndPoint, _communicationChannel);
            Send(builder.GetBytes(), true);

            StopProcessing = true;
            return Task.CompletedTask;
        }

        public async Task<bool> SendStatusReq()
        {
            ConnectionStatusRequest stat = new ConnectionStatusRequest();
            stat.Build(_receiveEndPoint, _communicationChannel);
            stat.SetChannelId(_communicationChannel);
            await Send(stat.GetBytes());
            return true;
        }


        private void ProcessReceivingMessages(UdpClient _udpClient)
        {
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

                        Debug.WriteLine("Telegram angekommen: " + knxResponse.ToString());


                        switch (knxResponse)
                        {
                            case ConnectResponse connectResponse:
                                if (connectResponse.Status == 0x00)
                                {
                                    _sequenceCounter = 0;
                                    _communicationChannel = connectResponse.CommunicationChannel;
                                    IsConnected = true;
                                    ConnectionChanged?.Invoke(IsConnected);
                                    SelfAddress = connectResponse.ConnectionResponseDataBlock.KnxAddress;
                                    Debug.WriteLine("Connected: Eigene Adresse: " + SelfAddress.ToString());
                                }
                                else
                                {
                                    Debug.WriteLine("Connected: Fehler: " + connectResponse.Status.ToString());
                                }

                                break;
                            case Builders.TunnelResponse tunnelResponse:
                                if (tunnelResponse.IsRequest && tunnelResponse.DestinationAddress != SelfAddress)
                                {
                                    Debug.WriteLine("Telegram erhalten das nicht mit der Adresse selbst zu tun hat!");
                                    Debug.WriteLine("Typ: " + tunnelResponse.APCI);
                                    Debug.WriteLine("Eigene Adresse: " + SelfAddress.ToString());
                                    break;
                                }

                                _sendMessages.Add(new Responses.TunnelResponse(0x06, 0x10, 0x0A, 0x04, _communicationChannel, tunnelResponse.SequenceCounter, 0x00).GetBytes());

                                if (tunnelResponse.APCI.ToString().EndsWith("Response"))
                                {
                                    TunnelRequest builder = new TunnelRequest();
                                    builder.Build(UnicastAddress.FromString("0.0.0"), tunnelResponse.SourceAddress, Parser.ApciTypes.Ack, tunnelResponse.SequenceNumber);
                                    builder.SetChannelId(_communicationChannel);
                                    builder.SetSequence(_sequenceCounter);
                                    Send(builder.GetBytes());
                                    _sequenceCounter++;
                                    //Debug.WriteLine("Got Response " + tunnelResponse.SequenceCounter + " . " + tunnelResponse.SequenceNumber);
                                    OnTunnelResponse?.Invoke(tunnelResponse);
                                }
                                else if (tunnelResponse.APCI == ApciTypes.Ack)
                                {
                                    OnTunnelAck?.Invoke(tunnelResponse);
                                }
                                else
                                {
                                    OnTunnelRequest?.Invoke(tunnelResponse);
                                }




                                break;

                            case SearchResponse searchResponse:
                                OnSearchResponse?.Invoke(searchResponse);
                                break;

                            case TunnelAckResponse tunnelAck:
                                //Do nothing
                                break;

                            case DisconnectResponse disconnectResponse:
                                IsConnected = false;
                                _communicationChannel = 0;
                                ConnectionChanged?.Invoke(IsConnected);
                                break;
                        }
                    }

                    Debug.WriteLine("Stopped Processing Messages " + _udpClient.Client.LocalEndPoint.ToString());
                    _udpClient.Close();
                    _udpClient.Dispose();
                }
                catch
                {

                }
            });
        }

        private void ProcessSendMessages()
        {
            Task.Run(() =>
            {

                foreach (var sendMessage in _sendMessages.GetConsumingEnumerable())
                {
                    if(sendMessage is byte[])
                    {

                        byte[] data = sendMessage as byte[];
                        foreach (UdpClient client in _udpList)
                        {
                            client.SendAsync(data, data.Length, _sendEndPoint);
                        }

                    } else if(sendMessage is IMessageRequest)
                    {
                        IMessageRequest message = sendMessage as IMessageRequest;
                        byte[] tosend = null;

                        foreach (UdpClient client in _udpList)
                        {
                            message.SetEndpoint(client.Client.LocalEndPoint as IPEndPoint);

                            switch (CurrentType)
                            {
                                case ProtocolTypes.Emi1:
                                    tosend = message.GetBytesEmi1();
                                    break;

                                case ProtocolTypes.Emi2:
                                    tosend = message.GetBytesEmi2();
                                    break;

                                case ProtocolTypes.cEmi:
                                    tosend = message.GetBytesCemi();
                                    break;

                                default:
                                    throw new Exception("Unbekanntes Protokoll");
                            }

                            client.SendAsync(tosend, tosend.Length, _sendEndPoint);
                        }
                    }
                }
            });
        }
    }
}
