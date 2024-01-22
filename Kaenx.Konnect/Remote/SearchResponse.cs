using Kaenx.Konnect.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;


namespace Kaenx.Konnect.Remote
{
    public class SearchResponse : IRemoteMessage
    {
        public int SequenceNumber { get; set; } = -1;

        public MessageCodes MessageCode { get; } = MessageCodes.SearchResponse;
        public int ChannelId { get; set; }
        public string Group { get; set; } = "";

        public List<IKnxInterface> Interfaces { get; set; } = new List<IKnxInterface>();


public ArraySegment<byte> GetBytes()
{
    byte[] list;

    // Serialize using Json.NET (Newtonsoft.Json)
    string json = JsonConvert.SerializeObject(Interfaces);
    list = Encoding.UTF8.GetBytes(json);

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

    // Deserialize using Newtonsoft.Json
    string json = Encoding.UTF8.GetString(buffer, 3, buffer.Length - 3);
    Interfaces = JsonConvert.DeserializeObject<List<IKnxInterface>>(json);
}

    }
}
