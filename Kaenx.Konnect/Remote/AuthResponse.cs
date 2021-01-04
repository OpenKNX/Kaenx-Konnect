using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaenx.Konnect.Remote
{
    public class AuthResponse : IRemoteMessage
    {
        public MessageCodes MessageCode { get; } = MessageCodes.AuthResponse;
        public string Group { get; set; }
        public int SequenceNumber { get; set; } = -1;
        public int ChannelId { get; set; } = 0;


        public AuthResponse() { }
        public AuthResponse(string code)
        {
            Group = code;
        }


        public void Parse(byte[] buffer)
        {
            SequenceNumber = buffer[1];
            Group = Encoding.UTF8.GetString(buffer.Skip(2).ToArray());
        }


        public ArraySegment<byte> GetBytes()
        {
            byte[] text = Encoding.UTF8.GetBytes(Group);
            byte[] bytes = new byte[text.Length + 2];
            text.CopyTo(bytes, 2);
            bytes[0] = Convert.ToByte(MessageCode);
            bytes[1] = Convert.ToByte(SequenceNumber);

            return new ArraySegment<byte>(bytes);
        }
    }
}
