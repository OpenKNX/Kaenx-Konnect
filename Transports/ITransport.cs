using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Enums;
using Kaenx.Konnect.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Connections.Transports
{
    internal interface ITransport : IDisposable
    {
        public bool IsAckRequired { get; }

        public Task SendAsync(byte[] data);
        public IPEndPoint GetLocalEndpoint();
        public HostProtocols GetProtocolType();

        public delegate Task ReceivedKnxMessage(object sender, byte[] data);
        public event ReceivedKnxMessage? OnReceived;
    }
}
