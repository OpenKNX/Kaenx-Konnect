using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Addresses
{
    public class UnicastAddress : IKnxAddress
    {
        public UnicastAddress(byte area, byte line, byte deviceAddress)
        {
            Area = area;
            Line = line;
            DeviceAddress = deviceAddress;
        }

        public UnicastAddress(int address)
            : this((byte)((address >> 12) & 0x0F), (byte)((address >> 8) & 0x0F), (byte)(address & 0xFF))
        {
        }

        public byte Area { get; }
        public byte Line { get; }
        public byte DeviceAddress { get; }

        public byte[] GetBytes()
        {
            return new[] { (byte)((Area << 4) | Line), DeviceAddress };
        }

        public int AsUInt16()
        {
            byte[] bytes = GetBytes();
            return BitConverter.ToUInt16(new byte[] { bytes[1], bytes[0] }, 0);
        }


        public static UnicastAddress FromByteArray(byte[] bytes)
        {
            return new UnicastAddress((byte)(bytes[0] >> 4), (byte)(bytes[0] & 0x0F), bytes[1]);
        }

        public static UnicastAddress FromString(string address)
        {
            var addressParts = address.Split('.');
            if (addressParts.Length != 3)
                throw new Exception("Invalid address string.");

            return new UnicastAddress(Convert.ToByte(addressParts[0]), Convert.ToByte(addressParts[1]),
              Convert.ToByte(addressParts[2]));
        }

        public override string ToString()
        {
            return Area.ToString() + "." + Line.ToString() + "." + DeviceAddress.ToString();
        }
    }
}
