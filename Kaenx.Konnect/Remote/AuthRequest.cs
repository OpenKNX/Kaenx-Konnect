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
        public string Group { get; set; } = "";
        public string Code { get; set; } = "";


        public AuthRequest() { }
        public AuthRequest(string auth, int sequence, string group = null, string code = null)
        {
            Auth = auth;
            SequenceNumber = sequence;
            Group = group ?? "";
            Code = code ?? "";
        }


        public void Parse(byte[] buffer)
        {
            SequenceNumber = buffer[1];

            Auth = Encoding.UTF8.GetString(buffer.Skip(4).Take(buffer[2]).ToArray());
            Group = Encoding.UTF8.GetString(buffer.Skip(4 + buffer[2]).Take(buffer[3]).ToArray());
            Code = Encoding.UTF8.GetString(buffer.Skip(4 + buffer[2] + buffer[3]).Take(buffer[4]).ToArray());
        }


        public ArraySegment<byte> GetBytes()
        {
            byte[] text = Encoding.UTF8.GetBytes(Auth);
            byte[] group = Encoding.UTF8.GetBytes(Group);
            byte[] code = Encoding.UTF8.GetBytes(Code);
            byte[] bytes = new byte[text.Length + group.Length + code.Length + 4];
            bytes[0] = Convert.ToByte(MessageCode);
            bytes[1] = Convert.ToByte(SequenceNumber);
            bytes[2] = Convert.ToByte(text.Length);
            bytes[3] = Convert.ToByte(group.Length);
            bytes[4] = Convert.ToByte(code.Length);

            text.CopyTo(bytes, 4);
            group.CopyTo(bytes, 4 + text.Length);
            code.CopyTo(bytes, 4 + text.Length + group.Length);

            return new ArraySegment<byte>(bytes);
        }
    }
}
