using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaenx.Konnect.Remote
{
    public class AuthRequest : IRemoteMessage
    {
        public MessageCodes MessageCode { get; } = MessageCodes.AuthRequest;
        public string Auth { get; set; }
        public int SequenceNumber { get; set; } = -1;
        public int ChannelId { get; set; } = 0;

        public AuthRequest() { }
        public AuthRequest(string auth, int sequence)
        {
            Auth = auth;
            SequenceNumber = sequence;
        }


        public void Parse(byte[] buffer)
        {
            SequenceNumber = buffer[1];
            Auth = Encoding.UTF8.GetString(buffer.Skip(2).ToArray());
        }


        public ArraySegment<byte> GetBytes()
        {
            byte[] text = Encoding.UTF8.GetBytes(Auth);
            byte[] bytes = new byte[text.Length + 2];
            text.CopyTo(bytes, 2);
            bytes[0] = Convert.ToByte(MessageCode);
            bytes[1] = Convert.ToByte(SequenceNumber);

            return new ArraySegment<byte>(bytes);
        }
    }
}
