using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Connections
{
    public interface IKnxConnection
    {
        public delegate void TunnelRequestHandler(Builders.TunnelResponse response);
        public event TunnelRequestHandler OnTunnelRequest;
        public event TunnelRequestHandler OnTunnelResponse;
        public event TunnelRequestHandler OnTunnelAck;

        public delegate void SearchResponseHandler(SearchResponse response);
        public event SearchResponseHandler OnSearchResponse;

        public delegate void ConnectionChangedHandler(bool isConnected);
        public event ConnectionChangedHandler ConnectionChanged;

        public bool IsConnected { get; set; }

        void Connect();
        void Disconnect();
        void SendStatusReq();
        void Send(byte[] data);
        byte Send(IRequestBuilder builder);
    }
}
