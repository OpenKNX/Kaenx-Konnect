using Kaenx.Konnect.Enums;
using Kaenx.Konnect.Telegram.Contents;
using Kaenx.Konnect.Telegram.IP.DIB;
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

        public HpaiContent? GetEndpoint()
        {
            return Contents.OfType<HpaiContent>().FirstOrDefault();
        }

        public ChannelInfoContent? GetChannelInfo()
        {
            return Contents.OfType<ChannelInfoContent>().FirstOrDefault();
        }

        public override void Parse(byte[] data)
        {
            IEnumerable<byte> _data = data;
            Header.Parse(data);
            _data = _data.Skip(Header.HeaderLength);

            ChannelInfoContent channelInfo = new ChannelInfoContent(_data.ToArray());
            ChannelId = channelInfo.ChannelId;
            ReturnCode = channelInfo.ReturnCode;
            Contents.Add(channelInfo);
            _data = _data.Skip(channelInfo.Length);

            if(ReturnCode != IpErrors.NoError)
                return;

            HpaiContent hpai = new HpaiContent(_data.ToArray());
            Contents.Add(hpai);
            _data = _data.Skip(hpai.Length);

            ConnectionResponseData connectionResponseData = new ConnectionResponseData(_data.ToArray());
            Contents.Add(connectionResponseData);
        }
    }
}
