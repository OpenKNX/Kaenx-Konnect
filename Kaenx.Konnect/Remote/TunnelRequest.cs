using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaenx.Konnect.Remote
{
    public class TunnelRequest : IRemoteMessage
    {
        public MessageCodes MessageCode { get; } = MessageCodes.TunnelRequest;
        public int SequenceNumber { get; set; } = -1;
        public int ChannelId { get; set; } = 0;
        public TunnelTypes Type { get; set; }
        private byte[] Data;

        public TunnelRequest() { }
        public TunnelRequest(byte[] data)
        {
            Data = data;
        }


        public void Parse(byte[] buffer)
        {
            SequenceNumber = buffer[1];
            Data = buffer.Skip(2).ToArray();
        }


        public ArraySegment<byte> GetBytes()
        {
            byte[] bytes = new byte[Data.Length + 2];
            bytes[0] = Convert.ToByte(MessageCode);
            bytes[1] = Convert.ToByte(SequenceNumber);
            Data.CopyTo(bytes, 2);

            return new ArraySegment<byte>(bytes);
        }
    }

    public enum TunnelTypes
    {
        Connect,
        Disconnect,
        State,
        Tunnel
    }
}
