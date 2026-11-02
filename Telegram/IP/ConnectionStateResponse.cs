using Kaenx.Konnect.Enums;
using Kaenx.Konnect.Telegram.Contents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.IP
{
    public class ConnectionStateResponse : IpTelegram
    {
        public byte ChannelId { get; private set; }
        public IpErrors ReturnCode { get; private set; }

        public ConnectionStateResponse(byte channelId, IpErrors returnCode)
            : base(ServiceIdentifiers.ConnectionStateResponse)
        {
            ChannelId = channelId;
            ReturnCode = returnCode;

            Contents.Add(new RawContent(new byte[] { ChannelId, (byte)ReturnCode }));
        }

        public ConnectionStateResponse(byte[] data)
            : base(ServiceIdentifiers.ConnectionStateResponse)
        {
            Parse(data);
        }

        public override void Parse(byte[] data)
        {
            IEnumerable<byte> _data = data;
            Header.Parse(data);
            _data = _data.Skip(Header.HeaderLength);

            ChannelId = _data.ElementAt(0);
            ReturnCode = (IpErrors)_data.ElementAt(1);
        }
    }
}
