using Kaenx.Konnect.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Kaenx.Konnect.Remote
{
    public class SearchResponse : IRemoteMessage
    {
        public int SequenceNumber { get; set; } = -1;

        public MessageCodes MessageCode { get; } = MessageCodes.SearchResponse;
        public int ChannelId { get; set; }

        public List<IKnxInterface> Interfaces { get; set; } = new List<IKnxInterface>();



        public ArraySegment<byte> GetBytes()
        {
            byte[] list = new byte[0];
            BinaryFormatter bf = new BinaryFormatter();

            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, Interfaces);
                list = ms.ToArray();
            }

            byte[] bytes = new byte[3 + list.Length];
            bytes[0] = Convert.ToByte(MessageCode);
            bytes[1] = Convert.ToByte(SequenceNumber);
            bytes[2] = Convert.ToByte(ChannelId);

            list.CopyTo(bytes, 3);

            return new ArraySegment<byte>(bytes);
        }

        public void Parse(byte[] buffer)
        {
            SequenceNumber = buffer[1];
            ChannelId = buffer[2];

            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(buffer.Skip(3).ToArray(), 0, buffer.Length - 3);
                memStream.Seek(0, SeekOrigin.Begin);
                Interfaces = (List<IKnxInterface>)binForm.Deserialize(memStream);
            }
        }
    }
}
