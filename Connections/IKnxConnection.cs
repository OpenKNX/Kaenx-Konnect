using Kaenx.Konnect.Builders;
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


        byte Send(byte[] data);
    }
}
