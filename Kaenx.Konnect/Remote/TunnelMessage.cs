using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaenx.Konnect.Remote
{
    public class TunnelMessage : IRemoteMessage
    {
        public MessageCodes MessageCode { get; } = MessageCodes.AuthRequest;
        public string Auth { get; set; }
        public int SequenceNumber { get; set; } = -1;
        public int ChannelId { get; set; } = 0;

        public string _key;
        public string _group;
        public string _code;


        public TunnelMessage() { }
        public TunnelMessage(string auth, int sequence)
        {
            Auth = auth;
            string[] part = auth.Split(':');
            _key = part[0];
            _group = part[1];
            _code = part[2];
            SequenceNumber = sequence;
        }


        public void Parse(byte[] buffer)
        {
            SequenceNumber = buffer[1];
            Auth = System.Text.Encoding.UTF8.GetString(buffer.Skip(2).ToArray());

            string[] part = Auth.Split(':');
            _key = part[0];
            _group = part[1];
            _code = part[2];
        }


        public ArraySegment<byte> GetBytes()
        {
            byte[] text = System.Text.Encoding.UTF8.GetBytes(Auth);
            byte[] bytes = new byte[text.Length + 2];
            text.CopyTo(bytes, 2);
            bytes[0] = Convert.ToByte(MessageCode);
            bytes[1] = Convert.ToByte(SequenceNumber);

            return new ArraySegment<byte>(bytes);
        }
    }
}
