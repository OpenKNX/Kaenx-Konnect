using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaenx.Konnect.Remote
{
    public class SearchRequest : IRemoteMessage
    {
        public int SequenceNumber { get; set; } = -1;

        public MessageCodes MessageCode { get; } = MessageCodes.SearchRequest;
        public int ChannelId { get; set; }
        public string Group { get; set; } = "";



        public ArraySegment<byte> GetBytes()
        {
            byte[] bytes = new byte[3];
            bytes[0] = Convert.ToByte(MessageCode);
            bytes[1] = Convert.ToByte(SequenceNumber);
            bytes[2] = Convert.ToByte(ChannelId);

            return new ArraySegment<byte>(bytes);
        }

        public void Parse(byte[] buffer)
        {
            SequenceNumber = buffer[1];
            ChannelId = buffer[2];
        }
    }
}
