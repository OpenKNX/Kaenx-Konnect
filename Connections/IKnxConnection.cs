using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Responses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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

        Task Connect();
        Task Disconnect();
        Task SendStatusReq();
        Task Send(byte[] data);
        Task<byte> Send(IRequestBuilder builder);
    }
}
