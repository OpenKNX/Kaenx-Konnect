using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Kaenx.Konnect.Remote
{
    public class CodesResponse : IRemoteMessage
    {
        public int SequenceNumber { get; set; } = -1;

        public MessageCodes MessageCode { get; } = MessageCodes.CodesResponse;
        public List<string> Codes { get; set; } = new List<string>();
        public int ChannelId { get; set; } = 0;
        public string Group { get; set; } = "";



        public ArraySegment<byte> GetBytes()
        {
            byte[] codes;
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, Codes);
                codes = ms.ToArray();
            }

            byte[] bytes = new byte[2 + codes.Length];
            bytes[0] = Convert.ToByte(MessageCode);
            bytes[1] = Convert.ToByte(SequenceNumber);

            codes.CopyTo(bytes, 2);

            return new ArraySegment<byte>(bytes);
        }

        public void Parse(byte[] buffer)
        {
            SequenceNumber = buffer[1];

            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(buffer.Skip(2).ToArray(), 0, buffer.Length - 2);
                memStream.Seek(0, SeekOrigin.Begin);
                Codes = (List<string>)binForm.Deserialize(memStream);
            }
        }
    }
}
