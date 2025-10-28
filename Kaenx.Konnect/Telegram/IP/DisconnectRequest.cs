using Kaenx.Konnect.Enums;
using Kaenx.Konnect.Telegram.Contents;
using Kaenx.Konnect.Telegram.IP.DIB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.IP
{
    public class DisconnectRequest : IpTelegram
    {
        public byte ChannelId { get; private set; }

        public DisconnectRequest(byte channelId, IPEndPoint endpoint, HostProtocols protocol)
            : base(ServiceIdentifiers.DisconnectRequest)
        {
            Initialize(channelId, endpoint, protocol);
        }

        public DisconnectRequest(byte channelId, string ipAddress, int ipPort, HostProtocols protocol)
            : base(ServiceIdentifiers.DisconnectRequest)
        {
            Initialize(channelId, new IPEndPoint(IPAddress.Parse(ipAddress), ipPort), protocol);
        }

        public DisconnectRequest(byte[] data)
            : base(ServiceIdentifiers.DisconnectRequest)
        {
            Parse(data);
        }

        private void Initialize(byte channelId, IPEndPoint endPoint, HostProtocols protocol)
        {
            Contents.Add(new ChannelInfoContent(channelId, 0x00)); // second byte is reserved
            var hpai = new HpaiContent(endPoint, protocol);
            Contents.Add(hpai); // Control
        }

        public override void Parse(byte[] data)
        {
            Header.Parse(data);
            IEnumerable<byte> _data = data.Skip(Header.HeaderLength);

            ChannelInfoContent channelInfo = new ChannelInfoContent(_data.ToArray());
            ChannelId = channelInfo.ChannelId;
            if(channelInfo.ReturnCode != 0x00)
                throw new Exception("DisconnectRequest reserved byte is not zero: " + channelInfo.ReturnCode.ToString());
        }
    }
}
