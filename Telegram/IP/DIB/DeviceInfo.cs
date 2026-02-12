using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.IP.DIB
{
    public class DeviceInfo : IContent
    {
        public int Length => 54;
        public DibTypes Type => DibTypes.DeviceInfo;
        public string FriendlyName { get; set; } = "";
        public KnxMediums Medium { get; set; } = KnxMediums.TP1;
        
        // Device Status
        public bool ProgrammingMode { get; set; } = false;

        public UnicastAddress UnicastAddress { get; set; } = new UnicastAddress(0xFFFF);
        public int ProjectNumber { get; set; } = 0;
        public int ProjectInstallationNumber { get; set; } = 0;
        public byte[] SerialNumber { get; set; } = new byte[6];
        public byte[] MacAddress { get; set; } = new byte[6];
        public IPAddress DeviceMulticastAddress { get; set; } = new IPAddress(new byte[] { 224, 0, 23, 12 });

        public DeviceInfo(byte[] data)
        {
            if (data.Length < Length)
                throw new ArgumentException($"Data length is less than expected {Length} bytes.");
            if (data[1] != (byte)DibTypes.DeviceInfo)
                throw new ArgumentException("Data does not represent a DeviceInfo DIB.");

            // Parsing logic to be implemented
            Medium = (KnxMediums)data[2];

            byte status = data[3];
            ProgrammingMode = (status & 0x01) != 0;
            UnicastAddress = UnicastAddress.FromByteArray(new byte[] { data[4], data[5] });
            ProjectNumber = (data[6] << 4) | (data[7] >> 4);
            ProjectInstallationNumber = data[7] & 0x0F;

            Array.Copy(data, 8, SerialNumber, 0, 6);

            DeviceMulticastAddress = new IPAddress(new byte[] { data[14], data[15], data[16], data[17] });

            Array.Copy(data, 18, MacAddress, 0, 6);

            FriendlyName = Encoding.ASCII.GetString(data, 24, 30).TrimEnd('\0');
        }

        public byte[] ToByteArray()
        {
            throw new NotImplementedException();
        }
    }
}
