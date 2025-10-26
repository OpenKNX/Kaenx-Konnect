using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.Contents
{
    public class HpaiContent : IContent
    {
        public IPEndPoint Endpoint { get;  private set; }
        public HostProtocols Protocol { get; private set; }

        public int Length => 8;

        public HpaiContent(IPEndPoint endpoint, HostProtocols protocol)
        {
            Endpoint = endpoint;
            Protocol = protocol;
        }

        public HpaiContent(byte[] data)
        {
            if (data.Length < Length)
                throw new ArgumentException($"Data length must be at least {Length} bytes.");
            if (data[0] != 0x08)
                throw new ArgumentException("Invalid HPAI length.");
            Protocol = (HostProtocols)data[1];
            byte[] ipBytes = data.Skip(2).Take(4).ToArray();
            byte[] portBytes = data.Skip(6).Take(2).Reverse().ToArray();
            IPAddress ipAddress = new IPAddress(ipBytes);
            ushort port = BitConverter.ToUInt16(portBytes, 0);
            Endpoint = new IPEndPoint(ipAddress, port);
        }

        public byte[] ToByteArray()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add(0x08); // Body Structure Length
            bytes.Add((byte)Protocol); // IPv4
            byte[] ip = Endpoint.Address.GetAddressBytes();
            bytes.AddRange(ip); // IP Address
            byte[] port = BitConverter.GetBytes((ushort)Endpoint.Port).Reverse().ToArray();
            bytes.AddRange(port); // IP Adress Port

            return bytes.ToArray();
        }
    }
}
