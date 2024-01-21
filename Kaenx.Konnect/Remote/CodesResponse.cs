using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;

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

            // Serialize using Newtonsoft.Json
            string json = JsonConvert.SerializeObject(Codes);
            codes = Encoding.UTF8.GetBytes(json);

            byte[] bytes = new byte[2 + codes.Length];
            bytes[0] = Convert.ToByte(MessageCode);
            bytes[1] = Convert.ToByte(SequenceNumber);

            codes.CopyTo(bytes, 2);

            return new ArraySegment<byte>(bytes);
        }

        public void Parse(byte[] buffer)
        {
            SequenceNumber = buffer[1];

            // Deserialize using Newtonsoft.Json
            string json = Encoding.UTF8.GetString(buffer, 2, buffer.Length - 2);
            Codes = JsonConvert.DeserializeObject<List<string>>(json);
        }
    }
}
