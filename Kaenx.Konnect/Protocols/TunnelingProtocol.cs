using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Connections.Transports;
using Kaenx.Konnect.EMI;
using Kaenx.Konnect.EMI.LData;
using Kaenx.Konnect.Enums;
using Kaenx.Konnect.Telegram.Contents;
using Kaenx.Konnect.Telegram.IP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Connections.Protocols
{
    internal class TunnelingProtocol : IProtocol
    {
        private CancellationTokenSource? _connectToken;
        private CancellationTokenSource? _confirmationToken;
        private byte _channelId { get; set; } = 0;
        private byte _sequenzeCounter { get; set; } = 0;
        private int _lastReceivedSequenceCounter = -1;

        private UnicastAddress? _localAddress = new UnicastAddress(0);
        public override UnicastAddress? LocalAddress { get { return _localAddress; } }

        private List<(int sequenceCounter, CancellationTokenSource tokenSource)> _ackWaitList = new List<(int, CancellationTokenSource)>();

        public TunnelingProtocol(ITransport transport)
            : base(transport)
        {
            transport.OnReceived += Connection_OnReceived;
        }

        private async Task Connection_OnReceived(object sender, byte[] data)
        {
            KnxIpHeader header = new KnxIpHeader(data);

            switch (header.ServiceIdentifier)
            {
                case ServiceIdentifiers.SearchResponse:
                    {
                        SearchResponse response = new SearchResponse(data);
                        InvokeReceivedService(response);
                        break;
                    }

                case ServiceIdentifiers.ConnectResponse:
                    {
                        ConnectResponse response = new ConnectResponse(data);
                        if (_connectToken != null)
                        {
                            _connectToken.Cancel();
                            _connectToken = null;
                        }
                        _channelId = response.ChannelId;
                        if (response.ReturnCode != IpErrors.NoError)
                        {
                            throw new Exception("ConnectResponse returned error: " + response.ReturnCode.ToString());
                        }
                        IsConnected = true;
                        InvokeReceivedService(response);
                        break;
                    }

                case ServiceIdentifiers.DisconnectRequest:
                    {
                        DisconnectRequest request = new DisconnectRequest(data);
                        InvokeReceivedService(request);
                        IsConnected = false;
                        break;
                    }

                case ServiceIdentifiers.DisconnectResponse:
                    {
                        DisconnectResponse response = new DisconnectResponse(data);
                        // TODO send DisconnectResponse
                        IsConnected = false;
                        InvokeReceivedService(response);
                        break;
                    }

                case ServiceIdentifiers.TunnelingRequest:
                    {
                        TunnelingRequest request = new TunnelingRequest(data);
                        bool ackRequired = false;
                        if(request.MessageCode == MessageCodes.L_Data_con)
                        {
                            if (_confirmationToken != null)
                            {
                                _confirmationToken.Cancel();
                                _confirmationToken = null;
                            }
                            ackRequired = true;
                        }
                        if(request.MessageCode == MessageCodes.L_Data_ind)
                        {
                            ackRequired = true;
                        }

                        if(ackRequired && _transport.IsAckRequired)
                        {
                            TunnelingAck ack = new TunnelingAck(request.GetConnectionHeader().ChannelId, request.GetConnectionHeader().SequenceCounter);
                            await SendAsync(ack);
                        }

                        if (request.GetConnectionHeader().SequenceCounter > _lastReceivedSequenceCounter)
                            _lastReceivedSequenceCounter = request.GetConnectionHeader().SequenceCounter;

                        if (request.MessageCode == MessageCodes.L_Data_ind)
                        {
                            EmiContent? emiContent = request.Contents.OfType<EmiContent>().FirstOrDefault();
                            if (emiContent == null)
                                return;

                            LDataBase? lDataBase = emiContent.Message as LDataBase;
                            if (lDataBase == null)
                                return;

                            InvokeReceivedMessage(lDataBase);
                        }

                        break;
                    }

                case ServiceIdentifiers.TunnelingAck:
                    {
                        Debug.WriteLine("Got Ack");
                        TunnelingAck ack = new TunnelingAck(data);
                        //InvokeReceivedService(ack);
                        CancellationTokenSource? source = _ackWaitList.FirstOrDefault(x => x.sequenceCounter == ack.ConnectionHeader.SequenceCounter).tokenSource;
                        if(source != null)
                        {
                            _ackWaitList.RemoveAll(x => x.sequenceCounter == ack.ConnectionHeader.SequenceCounter);
                            source.Cancel();
                        }
                        break;
                    }

                default:
                    throw new NotImplementedException("Unknown ServiceIdentifier: " + data[2].ToString("X2") + data[3].ToString("X2"));
            }
        }

        public override async Task SendAsync(IpTelegram ipTelegram)
        {
            string name = ipTelegram.GetType().Name;
            if (name == "RoutingIndication")
            {
                // We cannot send RoutingIndication via TunnelingProtocol
                throw new InvalidOperationException("Cannot send RoutingIndication via TunnelingProtocol");
            }
            await _transport.SendAsync(ipTelegram.ToByteArray());
        }

        public override async Task<int> SendAsync(LDataBase message)
        {
            if(!IsConnected)
                throw new InvalidOperationException("Not connected");

            TunnelingRequest request = new(message, _channelId, _sequenzeCounter);
            await WaitForAck(request);
            await WaitForConfirmation(_sequenzeCounter);
            _sequenzeCounter++;
            return _lastReceivedSequenceCounter + 1;
        }

        public override async Task Connect()
        {
            // TODO get real IPEndpoint
            IPEndPoint localEndpoint = new IPEndPoint(IPAddress.Parse("192.168.178.84"), GetLocalEndpoint().Port);
            ConnectRequest creq = new ConnectRequest(localEndpoint, HostProtocols.IPv4_UDP);
            _connectToken = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            await SendAsync(creq);
            try
            {
                await Task.Delay(5000, _connectToken.Token);
            }
            catch (TaskCanceledException)
            {
                // Connected
                return;
            }
            throw new TimeoutException("ConnectRequest timed out");
        }

        private async Task WaitForAck(TunnelingRequest request, CancellationTokenSource? token = null)
        {
            if (token == null)
                token = new CancellationTokenSource(TimeSpan.FromSeconds(3));

            _ackWaitList.Add((request.GetConnectionHeader().SequenceCounter, token));
            await _transport.SendAsync(request.ToByteArray());
            try
            {
                await Task.Delay(5000, token.Token);
            }
            catch (TaskCanceledException)
            {
                // Ack received
                return;
            }
            throw new TimeoutException("TunnelingAck timed out");
        }

        private async Task WaitForConfirmation(int sequenceCounter, CancellationTokenSource? token = null)
        {
            if (token == null)
                _confirmationToken = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            else
                _confirmationToken = token;

            try
                {
                    await Task.Delay(5000, _confirmationToken.Token);
                }
                catch (TaskCanceledException)
                {
                    // Confirmation received
                    return;
                }
            throw new TimeoutException("TunnelingConfirmation timed out");
        }

        public override int GetMaxApduLength()
        {
            return base.GetMaxApduLength();
        }
    }
}
