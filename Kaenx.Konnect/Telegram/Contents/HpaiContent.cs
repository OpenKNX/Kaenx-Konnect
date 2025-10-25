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
        private IPEndPoint _endpoint;
        private HostProtocols _protocol;

        public int Length => 8;

        public HpaiContent(IPEndPoint endpoint, HostProtocols protocol)
        {
            _endpoint = endpoint;
            _protocol = protocol;
        }

        public HpaiContent(byte[] data)
        {
            if (data.Length < Length)
                throw new ArgumentException($"Data length must be at least {Length} bytes.");
            if (data[0] != 0x08)
                throw new ArgumentException("Invalid HPAI length.");
            _protocol = (HostProtocols)data[1];
            byte[] ipBytes = data.Skip(2).Take(4).ToArray();
            byte[] portBytes = data.Skip(6).Take(2).Reverse().ToArray();
            IPAddress ipAddress = new IPAddress(ipBytes);
            ushort port = BitConverter.ToUInt16(portBytes, 0);
            _endpoint = new IPEndPoint(ipAddress, port);
        }

        public byte[] ToByteArray()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add(0x08); // Body Structure Length
            bytes.Add((byte)_protocol); // IPv4
            byte[] ip = _endpoint.Address.GetAddressBytes();
            bytes.AddRange(ip); // IP Address
            byte[] port = BitConverter.GetBytes((ushort)_endpoint.Port).Reverse().ToArray();
            bytes.AddRange(port); // IP Adress Port

            return bytes.ToArray();
        }
    }
}
