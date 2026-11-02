using Kaenx.Konnect.Enums;
using Kaenx.Konnect.Telegram.Contents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.IP
{
    public class TunnelingAck : IpTelegram
    {
        public ConnectionHeaderContent ConnectionHeader { get; private set; }

        public TunnelingAck(uint ChannelId, byte SequenceCounter)
            : base(ServiceIdentifiers.TunnelingAck)
        {
            ConnectionHeader = new ConnectionHeaderContent(ChannelId, SequenceCounter);
            Contents.Add(ConnectionHeader);
        }
        public TunnelingAck(byte[] data)
            : base(ServiceIdentifiers.TunnelingAck)
        {
            Parse(data);
        }

        public override void Parse(byte[] data)
        {
            Header.Parse(data);
            ConnectionHeader = new ConnectionHeaderContent(data.Skip(6).Take(4).ToArray());
        }
    }
}
