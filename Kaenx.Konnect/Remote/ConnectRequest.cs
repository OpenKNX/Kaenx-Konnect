using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaenx.Konnect.Remote
{
    public class ConnectRequest : IRemoteMessage
    {
        public int SequenceNumber { get; set; } = -1;
        public int ChannelId { get; set; } = 0;

        public MessageCodes MessageCode { get; } = MessageCodes.ConnectRequest;
        public string Code { get; set; } = "";
        public string Group { get; set; }



        public ArraySegment<byte> GetBytes()
        {
            byte[] group = Encoding.UTF8.GetBytes(Group);
            byte[] code = Encoding.UTF8.GetBytes(Code);


            byte[] bytes = new byte[4 + code.Length + group.Length];
            bytes[0] = Convert.ToByte(MessageCode);
            bytes[1] = Convert.ToByte(SequenceNumber);



            bytes[2] = Convert.ToByte(group.Length);
            bytes[3] = Convert.ToByte(code.Length);

            group.CopyTo(bytes, 4);
            code.CopyTo(bytes, 4 + group.Length);

            return new ArraySegment<byte>(bytes);
        }

        public void Parse(byte[] buffer)
        {
            SequenceNumber = buffer[1];

            if (buffer[2] > 0) Group = Encoding.UTF8.GetString(buffer.Skip(4).Take(buffer[2]).ToArray());
            if (buffer[3] > 0) Code = Encoding.UTF8.GetString(buffer.Skip(4 + buffer[2]).Take(buffer[3]).ToArray());
        }
    }
}
