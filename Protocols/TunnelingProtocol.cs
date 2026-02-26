using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Connections.Transports;
using Kaenx.Konnect.EMI;
using Kaenx.Konnect.EMI.LData;
using Kaenx.Konnect.Enums;
using Kaenx.Konnect.Telegram.Contents;
using Kaenx.Konnect.Telegram.IP;
using Kaenx.Konnect.Telegram.IP.DIB;
using Kaenx.Konnect.Exceptions;
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
        private int _confirmationTokenSequenceCounter = -1;

        private uint _channelId { get; set; } = 0;
        private byte _sequenzeCounter { get; set; } = 0;
        private int _lastReceivedSequenceCounter = -1;
        private IpErrors _connectionResponseCode = IpErrors.NoError;
        private Timer? _keepAliveTimer = null;
        //private ITransport _transport;

        private UnicastAddress? _localAddress = new UnicastAddress(0);
        public override UnicastAddress? LocalAddress { get { return _localAddress; } }

        private Dictionary<int, CancellationTokenSource> _ackWaitList = new Dictionary<int, CancellationTokenSource>();

        public TunnelingProtocol(ITransport transport)
            : base(transport)
        {
            transport.OnReceived += Connection_OnReceived;
            _transport = transport;
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
                        _connectionResponseCode = response.ReturnCode;
                        if (_connectToken != null)
                        {
                            _connectToken.Cancel();
                            _connectToken = null;
                        }
                        _channelId = response.ChannelId;
                        if (response.ReturnCode != IpErrors.NoError)
                        {
                            //throw new InterfaceException("ConnectResponse returned error: " + response.ReturnCode.ToString());
                            IsConnected = false;
                            break;
                        }
                        ConnectionResponseData? connectionResponseData = response.Contents.OfType<ConnectionResponseData>().FirstOrDefault();
                        if (connectionResponseData == null)
                            throw new InterfaceException("ConnectResponse does not contain ConnectionResponseData");
                        _localAddress = connectionResponseData.LocalAddress;
                        IsConnected = true;
                        InvokeReceivedService(response);
                        break;
                    }

                case ServiceIdentifiers.ConnectionStateResponse:
                    {
                        ConnectionStateResponse response = new ConnectionStateResponse(data);
                        _connectionResponseCode = response.ReturnCode;
                        if (_connectToken != null)
                        {
                            _connectToken.Cancel();
                            _connectToken = null;
                        }
                        InvokeReceivedService(response);
                        break;
                    }

                case ServiceIdentifiers.DisconnectRequest:
                    {
                        DisconnectRequest request = new DisconnectRequest(data);
                        InvokeReceivedService(request);
                        IsConnected = false;
                        if (_keepAliveTimer != null)
                        {
                            _keepAliveTimer.Dispose();
                            _keepAliveTimer = null;
                        }
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
                            Debug.WriteLine($"Got confirmation XX:{request.GetConnectionHeader().SequenceCounter}");
                            if (_confirmationToken != null && _confirmationTokenSequenceCounter == request.GetConnectionHeader().SequenceCounter)
                            {
                                _confirmationToken.Cancel();
                                _confirmationToken = null;
                            } else
                            {
                                
                            }
                            ackRequired = true;
                        }
                        if(request.MessageCode == MessageCodes.L_Data_ind)
                        {
                            ackRequired = true;
                        }

                        if (ackRequired && _transport.IsAckRequired)
                        {
                            Debug.WriteLine("Sending ack");
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
                        Debug.WriteLine("Got TunnelingAck");
                        TunnelingAck ack = new TunnelingAck(data);
                        //InvokeReceivedService(ack);
                        if(_ackWaitList.ContainsKey(ack.ConnectionHeader.SequenceCounter))
                        {
                            _ackWaitList[ack.ConnectionHeader.SequenceCounter].Cancel();
                            _ackWaitList.Remove(ack.ConnectionHeader.SequenceCounter);
                        }
                        break;
                    }

                default:
                    throw new NotImplementedException("Unknown ServiceIdentifier: " + data[2].ToString("X2") + data[3].ToString("X2"));
            }
        }

        public async void KeepAliveCallback(object? state)
        {
            ConnectionStateRequest csreq = new ConnectionStateRequest(_channelId, GetLocalEndpoint(), _transport.GetProtocolType());
            _connectToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await SendAsync(csreq);

            try
            {
                await Task.Delay(3000, _connectToken.Token);
            }
            catch (TaskCanceledException)
            {
                if (_connectionResponseCode != IpErrors.NoError)
                    throw new InterfaceException("ConnectionStateResponse returned " + _connectionResponseCode.ToString());
                // Everything ok
                return;
            }
            
            throw new TimeoutException("ConnectionStateRequest timed out");
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
                throw new InterfaceNotConnectedException();

            byte sequenceCounter = _sequenzeCounter;
            TunnelingRequest request = new(message, _channelId, sequenceCounter);

            _ackWaitList.Add(sequenceCounter, new CancellationTokenSource());
            _confirmationToken = new CancellationTokenSource();
            _confirmationTokenSequenceCounter = sequenceCounter;
            Debug.WriteLine($"Starting confToken XX:{sequenceCounter}");
            await WaitForAck(request);
            _sequenzeCounter++;
            await WaitForConfirmation(sequenceCounter);
            return _lastReceivedSequenceCounter + 1;
        }

        public override async Task Connect()
        {
            _sequenzeCounter = 0;
            ConnectRequest creq = new ConnectRequest(GetLocalEndpoint(), _transport.GetProtocolType());
            _connectToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await SendAsync(creq);
            try
            {
                await Task.Delay(3000, _connectToken.Token);
            }
            catch (TaskCanceledException)
            {
                if (_connectionResponseCode != IpErrors.NoError)
                    throw new InterfaceException("ConnectResponse returned " + _connectionResponseCode.ToString());
                // Connected
                _keepAliveTimer = new Timer(KeepAliveCallback, null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60) );
                return;
            }
            throw new TimeoutException("ConnectRequest timed out");
        }

        public override async Task Disconnect()
        {
            if (!IsConnected)
                return;

            DisconnectRequest dreq = new DisconnectRequest(_channelId, GetLocalEndpoint(), _transport.GetProtocolType());
            await SendAsync(dreq);
            IsConnected = false;
            if (_keepAliveTimer != null)
            {
                _keepAliveTimer.Dispose();
                _keepAliveTimer = null;
            }
        }

        private async Task WaitForAck(TunnelingRequest request, TimeSpan? timeout = null)
        {
            TimeSpan delay = timeout ?? TimeSpan.FromSeconds(3);
            await _transport.SendAsync(request.ToByteArray());
            if(!_transport.IsAckRequired)
                return;

            try
            {
                CancellationTokenSource source = _ackWaitList[request.GetConnectionHeader().SequenceCounter];
                // We already received the ack
                if (source.Token.IsCancellationRequested)
                    return;

                await Task.Delay(delay, source.Token);

                if (_ackWaitList.ContainsKey(request.GetConnectionHeader().SequenceCounter))
                    _ackWaitList.Remove(request.GetConnectionHeader().SequenceCounter);
            }
            catch (TaskCanceledException)
            {
                // Ack received
                return;
            }
            throw new InterfaceException($"TunnelingAck timed out #XX:{request.GetConnectionHeader().SequenceCounter}");
        }

        private async Task WaitForConfirmation(int sequenceCounter, TimeSpan? timeout = null)
        {
            TimeSpan delay = timeout ?? TimeSpan.FromSeconds(3);
            try
            {
                // We already received the confirmation
                if (_confirmationToken == null)
                    return;

                Debug.WriteLine($"Start waiting conf XX:{sequenceCounter}");
                await Task.Delay(delay, _confirmationToken.Token);
                _confirmationToken = null;
                _confirmationTokenSequenceCounter = -1;
            }
            catch (TaskCanceledException)
            {
                // Confirmation received
                _confirmationToken = null;
                _confirmationTokenSequenceCounter = -1;
                return;
            }
            throw new InterfaceException($"TunnelingConfirmation timed out #XX:{sequenceCounter}");
        }

        public override int GetMaxApduLength()
        {
            return base.GetMaxApduLength();
        }

        public HostProtocols GetProtocolType()
        {
            return _transport.GetProtocolType();
        }

        public uint GetChannelId()
        {
            return _channelId;
        }
    }
}
