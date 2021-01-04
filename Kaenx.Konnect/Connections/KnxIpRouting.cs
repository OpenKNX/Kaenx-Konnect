using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Messages;
using Kaenx.Konnect.Messages.Request;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Connections
{
    public class KnxIpRouting : IKnxConnection
    {
        public bool IsConnected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ConnectionErrors LastError { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public UnicastAddress PhysicalAddress { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event IKnxConnection.TunnelRequestHandler OnTunnelRequest;
        public event IKnxConnection.TunnelResponseHandler OnTunnelResponse;
        public event IKnxConnection.TunnelAckHandler OnTunnelAck;
        public event IKnxConnection.SearchResponseHandler OnSearchResponse;
        public event IKnxConnection.ConnectionChangedHandler ConnectionChanged;

        public Task Connect()
        {
            throw new NotImplementedException();
        }

        public Task Disconnect()
        {
            throw new NotImplementedException();
        }

        public Task Send(byte[] data, bool ignoreConnected = false)
        {
            throw new NotImplementedException();
        }

        public Task<byte> Send(IMessage message, bool ignoreConnected = false)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendStatusReq()
        {
            throw new NotImplementedException();
        }
    }
}
