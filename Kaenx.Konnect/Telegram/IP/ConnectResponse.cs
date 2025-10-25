using Kaenx.Konnect.Enums;
using Kaenx.Konnect.Telegram.Contents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.IP
{
    public class ConnectResponse : IpTelegram
    {
        public byte ChannelId { get; private set; }
        public IpErrors ReturnCode { get; private set; }

        public ConnectResponse(byte channelId, IpErrors returnCode)
            : base(ServiceIdentifiers.ConnectResponse)
        {
            ChannelId = channelId;
            ReturnCode = returnCode;
        }

        public ConnectResponse(byte[] data)
            : base(ServiceIdentifiers.ConnectResponse)
        {
            Parse(data);
        }

        public override void Parse(byte[] data)
        {
            IEnumerable<byte> _data = data;
            Header.Parse(data);
            _data = _data.Skip(Header.HeaderLength);

            ChannelInfo channelInfo = new ChannelInfo(_data.ToArray());
            ChannelId = channelInfo.ChannelId;
            ReturnCode = channelInfo.ReturnCode;
            _data = _data.Skip(channelInfo.Length);
        }
    }
}
