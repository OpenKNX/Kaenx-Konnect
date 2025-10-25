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

        public RoutingProtocol(ITransport connection)
            : base(connection)
        {
            connection.OnReceived += Connection_OnReceived;
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
