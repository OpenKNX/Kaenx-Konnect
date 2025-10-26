using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.Contents
{
    public class ChannelInfoContent : IContent
    {
        public int Length => 2;

        public byte ChannelId { get; private set; }
        public IpErrors ReturnCode { get; private set; }

        public ChannelInfoContent(byte channelId, IpErrors returnCode)
        {
            ChannelId = channelId;
            ReturnCode = returnCode;
        }

        public ChannelInfoContent(byte[] data)
        {
            if (data.Length < Length)
                throw new ArgumentException("Data does not represent a ChannelInfoContent content.");
            ChannelId = data[0];
            ReturnCode = (IpErrors)data[1];
        }

        public byte[] ToByteArray()
        {
            return new byte[] { (byte)ReturnCode, ChannelId };
        }
    }
}
