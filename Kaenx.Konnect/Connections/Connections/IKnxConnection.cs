using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.EMI.DataMessages;
using Kaenx.Konnect.EMI.LData;
using Kaenx.Konnect.Telegram.IP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Connections.Connections
{
    public interface IKnxConnection
    {
        public delegate void ReceivedMessage(LDataBase message);
        public event ReceivedMessage? OnReceivedMessage;

        public delegate void ReceivedService(IpTelegram ipTelegram);
        public event ReceivedService? OnReceivedService;

        public Task<int> SendAsync(LDataBase message);

        public Task SendAsync(byte[] data);

        public Task Connect();

        public Task Disconnect();
    }
}
