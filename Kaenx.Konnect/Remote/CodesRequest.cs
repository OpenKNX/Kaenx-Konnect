using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaenx.Konnect.Remote
{
    public class CodesRequest : IRemoteMessage
    {
        public int SequenceNumber { get; set; } = -1;
        public int ChannelId { get; set; } = 0;

        public MessageCodes MessageCode { get; } = MessageCodes.CodesRequest;
        public CodeRequestActions Action { get; set; } = CodeRequestActions.List;
        public string Code { get; set; } = "";
        public string Group { get; set; }



        public ArraySegment<byte> GetBytes()
        {
            byte[] group = Encoding.UTF8.GetBytes(Group);
            byte[] code = Encoding.UTF8.GetBytes(Code);

            byte[] bytes = new byte[5 + code.Length + group.Length];
            bytes[0] = Convert.ToByte(MessageCode);
            bytes[1] = Convert.ToByte(SequenceNumber);
            bytes[2] = Convert.ToByte(Action);

            bytes[3] = Convert.ToByte(group.Length);
            bytes[4] = Convert.ToByte(code.Length);

            group.CopyTo(bytes, 5);
            code.CopyTo(bytes, 5 + group.Length);

            return new ArraySegment<byte>(bytes);
        }

        public void Parse(byte[] buffer)
        {
            SequenceNumber = buffer[1];
            Action = (CodeRequestActions)buffer[2];

            if (buffer[3] > 0) Group = Encoding.UTF8.GetString(buffer.Skip(5).Take(buffer[3]).ToArray());
            if (buffer[4] > 0) Code = Encoding.UTF8.GetString(buffer.Skip(5 + buffer[3]).Take(buffer[4]).ToArray());
        }
    }

    public enum CodeRequestActions
    {
        List,
        Create,
        Remove
    }
}
