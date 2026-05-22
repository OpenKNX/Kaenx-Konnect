using Kaenx.Konnect.Addresses;
using System;

namespace Kaenx.Konnect.Classes
{
    public class KnxDeviceInfo
    {
        public UnicastAddress IndividualAddress { get; set; }
        public byte[] Descriptor { get; set; } = Array.Empty<byte>(); // MaskVersion
        public bool ProgrammingMode { get; set; }
        public byte[] SerialNumber { get; set; } = Array.Empty<byte>();
        public byte[] ManufacturerId { get; set; } = Array.Empty<byte>();

        // Neu
        public ushort DeviceType { get; set; }  // Applikationsprogramm-Typ
        public byte AppVersion { get; set; }  // z.B. 0x01 → V0.1
        public byte RunError { get; set; }  // Ausführungsfehler ($FD = OK)
        public byte PeiType { get; set; }  // Hardware PEI Typ
        public byte LoadState { get; set; }  // Ausführungszustand (46)
        public ushort MaskVersion { get; set; }  // 0x0012
    }
}