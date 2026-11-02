using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.IP
{
    public abstract class IpTelegram
    {
        public KnxIpHeader Header { get; }
        public ServiceIdentifiers ServiceIdentifier { get; }
        public List<IContent> Contents { get; } = new List<IContent>();


        public IpTelegram(ServiceIdentifiers serviceIdentifier)
        {
            ServiceIdentifier = serviceIdentifier;
            Header = new KnxIpHeader(serviceIdentifier);
        }

        public abstract void Parse(byte[] data);

        public byte[] ToByteArray()
        {
            List<byte> bytes = new List<byte>();
            // Header
            bytes.Add((byte)Header.HeaderLength);
            bytes.Add((byte)Header.ProtocolVersion);
            bytes.AddRange(BitConverter.GetBytes((ushort)Header.ServiceIdentifier).Reverse());
            bytes.Add(0x00); // Placeholder for length
            bytes.Add(0x00); // Placeholder for length
            // Contents
            foreach (var content in Contents)
            {
                bytes.AddRange(content.ToByteArray());
            }

            Header.TotalLength = Header.HeaderLength + Contents.Sum(c => c.Length);
            byte[] length = BitConverter.GetBytes((ushort)Header.TotalLength).Reverse().ToArray();
            bytes[4] = length[0];
            bytes[5] = length[1];

            return bytes.ToArray();
        }
    }
}