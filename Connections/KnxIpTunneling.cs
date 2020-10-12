﻿using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Parser;
using Kaenx.Konnect.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
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

        private byte _communicationChannel;
        private bool StopProcessing = false;
        private byte _sequenceCounter = 0;
        private UnicastAddress SelfAddress;

        private readonly IPEndPoint _receiveEndPoint;
        private readonly IPEndPoint _sendEndPoint;
        private UdpClient _udpClient;
        private readonly BlockingCollection<byte[]> _sendMessages;
        private readonly ReceiverParserDispatcher _receiveParserDispatcher;

        public KnxIpTunneling(IPEndPoint sendEndPoint)
        {
            Port = GetFreePort();

            _sendEndPoint = sendEndPoint;
            _receiveEndPoint = new IPEndPoint(IPAddress.Any, Port);
            _udpClient = new UdpClient(_receiveEndPoint);
            _receiveParserDispatcher = new ReceiverParserDispatcher();
            _sendMessages = new BlockingCollection<byte[]>();

            ProcessSendMessages();
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
                throw new Exception("Roflkopter");

            _sendMessages.Add(data);

            return Task.CompletedTask;
        }

        public Task<byte> Send(IRequestBuilder builder, bool ignoreConnected = false)
        {
            if (!ignoreConnected && !IsConnected)
                throw new Exception("Roflkopter");

            var seq = _sequenceCounter;
            builder.SetChannelId(_communicationChannel);
            builder.SetSequence(_sequenceCounter);
            _sequenceCounter++;
            byte[] data = builder.GetBytes();

            _sendMessages.Add(data);

            return Task.FromResult(seq);
        }





        public Task Connect()
        {
            ConnectionRequest builder = new ConnectionRequest();
            builder.Build(_receiveEndPoint, 0x00);
            _sendMessages.Add(builder.GetBytes());
            return Task.CompletedTask;
        }

        public Task Disconnect()
        {
            if (!IsConnected)
                return Task.CompletedTask;

            DisconnectRequest builder = new DisconnectRequest();
            builder.Build(_receiveEndPoint, _communicationChannel);
            _sendMessages.Add(builder.GetBytes());

            StopProcessing = true;
            return Task.CompletedTask;
        }

        public Task<bool> SendStatusReq()
        {
            ConnectionStatusRequest stat = new ConnectionStatusRequest();
            stat.Build(_receiveEndPoint, _communicationChannel);
            Send(stat.GetBytes());
            return Task.FromResult(false);
        }



        private void ProcessSendMessages()
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
                                }
                                else
                                {

                                }

                                break;
                            case Builders.TunnelResponse tunnelResponse:
                                if (tunnelResponse.IsRequest && tunnelResponse.DestinationAddress != SelfAddress)
                                    break;

                                _sendMessages.Add(new Responses.TunnelResponse(0x06, 0x10, 0x0A, 0x04, _communicationChannel, tunnelResponse.SequenceCounter, 0x00).GetBytes());

                                if (tunnelResponse.APCI.ToString().EndsWith("Response"))
                                {
                                    TunnelRequest builder = new TunnelRequest();
                                    builder.Build(UnicastAddress.FromString("0.0.0"), tunnelResponse.SourceAddress, Parser.ApciTypes.Ack, tunnelResponse.SequenceNumber);
                                    Send(builder);
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

            Task.Run(() =>
            {

                foreach (var sendMessage in _sendMessages.GetConsumingEnumerable())
                {

                    _udpClient.SendAsync(sendMessage, sendMessage.Length, _sendEndPoint);
                }
            });
        }
    }
}
