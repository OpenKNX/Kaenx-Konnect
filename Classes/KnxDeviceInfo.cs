using Kaenx.Konnect.Addresses;
using System;

namespace Kaenx.Konnect.Classes
{
    public class KnxDeviceInfo
    {
        public UnicastAddress IndividualAddress { get; set; } = new UnicastAddress(0xFFFF);
        public byte[] Descriptor { get; set; } = Array.Empty<byte>();
        public bool ProgrammingMode { get; set; }
        public byte[] SerialNumber { get; set; } = Array.Empty<byte>();
        public byte[] ManufacturerId { get; set; } = Array.Empty<byte>();

        public string DescriptorHex =>
            Descriptor.Length > 0 ? $"0x{BitConverter.ToString(Descriptor).Replace("-", "")}" : "";

        public string SerialNumberHex =>
            BitConverter.ToString(SerialNumber).Replace("-", "");

        public string ManufacturerIdHex =>
            BitConverter.ToString(ManufacturerId).Replace("-", "");
    }
}