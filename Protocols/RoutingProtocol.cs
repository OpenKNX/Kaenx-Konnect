using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Connections.Transports;
using Kaenx.Konnect.EMI.LData;
using Kaenx.Konnect.Telegram.IP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Connections.Protocols
{
    internal class RoutingProtocol : IProtocol
    {
        public override bool IsConnected { get { return true; } }

        private UnicastAddress? _sourceAddress;
        public override UnicastAddress? LocalAddress { get { return _sourceAddress; } }

        public RoutingProtocol(UnicastAddress sourceAddress, ITransport connection)
            : base(connection)
        {
            connection.OnReceived += Connection_OnReceived;
            _sourceAddress = sourceAddress;
        }

        private Task Connection_OnReceived(object sender, byte[] data)
        {
            throw new NotImplementedException();
        }

        public override async Task SendAsync(IpTelegram ipTelegram)
        {
            await _transport.SendAsync(ipTelegram.ToByteArray());
        }

        public override async Task<int> SendAsync(LDataBase message)
        {
            //TunnelingRequest request = new TunnelingRequest(message);
            //await _connection.SendAsync(request.ToByteArray());
            return 0;
        }

        public override Task Connect()
        {
            // Nothing to do
            return Task.CompletedTask;
        }
    }
}
