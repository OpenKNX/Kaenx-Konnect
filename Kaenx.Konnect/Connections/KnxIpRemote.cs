using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Messages.Request;
using Kaenx.Konnect.Remote;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Kaenx.Konnect.Connections
{
    public class KnxIpRemote : IKnxConnection
    {
        public bool IsConnected
        {
            get { return socket.State == WebSocketState.Open; }
            set { }
        }
        public ConnectionErrors LastError { get; set; }
        public UnicastAddress PhysicalAddress { get; set; } = UnicastAddress.FromString("1.1.255");
        public int SequenceNumber
        {
            get { return _sequenceNumber; }
            set
            {
                _sequenceNumber = value;
                if (_sequenceNumber > 255)
                    _sequenceNumber = 0;
            }
        }

        public event IKnxConnection.TunnelRequestHandler OnTunnelRequest;
        public event IKnxConnection.TunnelResponseHandler OnTunnelResponse;
        public event IKnxConnection.TunnelAckHandler OnTunnelAck;
        public event IKnxConnection.SearchResponseHandler OnSearchResponse;
        public event IKnxConnection.ConnectionChangedHandler ConnectionChanged;

        private ClientWebSocket socket { get; set; } = new ClientWebSocket();
        private CancellationTokenSource ReceiveTokenSource { get; set; }
        private Dictionary<int, IRemoteMessage> Responses { get; set; } = new Dictionary<int, IRemoteMessage>();

        private int _sequenceNumber = 0;
        private byte _sequenceCounter = 0;
        private byte _communicationChannel;
    

        public KnxIpRemote(string hostname, string authentication)
        {

            for(int i = 0; i <256; i++)
            {
                Responses.Add(i, null);
            }
        }


        public async Task Connect()
        {
            
        }

        public async Task Disconnect()
        {
            
        }

        public Task Send(byte[] data, bool ignoreConnected = false)
        {
            throw new NotImplementedException();
        }

        public Task<byte> Send(IMessageRequest message, bool ignoreConnected = false)
        {
            if (!ignoreConnected && !IsConnected)
                throw new Exception("Roflkopter 2");

            var seq = _sequenceCounter;
            message.SequenceCounter = _sequenceCounter;
            message.ChannelId = _communicationChannel;
            _sequenceCounter++;
            //_sendMessages.Add(message);

            return Task.FromResult(seq);
        }

        public Task<bool> SendStatusReq()
        {
            throw new NotImplementedException();
        }


        


        
    }
}
