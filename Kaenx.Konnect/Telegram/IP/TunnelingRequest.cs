using Kaenx.Konnect.EMI;
using Kaenx.Konnect.EMI.LData;
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
    public class TunnelingRequest : IpTelegram
    {
        public MessageCodes MessageCode { get; private set; }

        public TunnelingRequest(IEmiMessage message, byte ChannelId, byte SequenceCounter) 
            : base(ServiceIdentifiers.TunnelingRequest)
        {
            Contents.Add(new ConnectionHeaderContent(ChannelId, SequenceCounter));
            Contents.Add(new EmiContent(message, ExternalMessageInterfaces.cEmi));

            //Contents.Add(new MessageContent(message, ExternalMessageInterfaces.cEmi));
        }

        public TunnelingRequest(byte[] data)
            : base(ServiceIdentifiers.TunnelingRequest)
        {
            Parse(data);
        }

        public override void Parse(byte[] data)
        {
            Header.Parse(data);
            IEnumerable<byte> _data = data.Skip(Header.HeaderLength);

            ConnectionHeaderContent ConnectionHeader = new ConnectionHeaderContent(_data.ToArray());
            Contents.Add(ConnectionHeader);
            _data = _data.Skip(ConnectionHeader.Length);

            MessageCode = (MessageCodes)_data.First();

            EmiContent content = new EmiContent(_data.ToArray(), ExternalMessageInterfaces.cEmi);
            Contents.Add(content);
        }

        public ConnectionHeaderContent GetConnectionHeader()
        {
            return Contents.OfType<ConnectionHeaderContent>().First();
        }
    }
}
