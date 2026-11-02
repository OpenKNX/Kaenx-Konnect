using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.Contents
{
    public class ConnectionHeaderContent : IContent
    {
        public uint ChannelId { get; private set; }
        public byte SequenceCounter { get; private set; }
        public IpErrors ReturnCode { get; private set; }
        public int Length => 4;

        public ConnectionHeaderContent(uint channelId, byte sequenceCounter)
        {
            if(channelId > 255)
                throw new ArgumentOutOfRangeException(nameof(channelId), "ChannelId must be between 0 and 255.");
            ChannelId = channelId;
            SequenceCounter = sequenceCounter;
        }

        public ConnectionHeaderContent(byte[] data)
        {
            Parse(data);
        }

        public void Parse(byte[] data)
        {
            if (data[0] != 4)
                throw new Exception("ConnectionHeaderContent has not expected length: " + data[0]);

            if (data[3] != 0)
                throw new Exception("Reserved byte is not zero: " + data[3]);

            ChannelId = data[1];
            SequenceCounter = data[2];
            ReturnCode = (IpErrors)data[3];
        }

        public byte[] ToByteArray()
        {
            return new byte[] { (byte)Length, (byte)ChannelId, SequenceCounter , 0x00 };
        }
    }
}
