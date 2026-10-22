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


        //NEW
        public string? OrderNumber { get; set; }
        public string? HardwareType { get; set; }
        public string? FirmwareVersion { get; set; }
        public string? IpName { get; set; }
        public string? MacAddress { get; set; }
        public string? IpAddress { get; set; }
        public string? SubnetMask { get; set; }
        public string? DefaultGateway { get; set; }
        public string? RoutingMulticastAddress { get; set; }
        public string? AppProgramName { get; set; }
        public string? AppProgramLoadState { get; set; }
        public string? App2Name { get; set; }
        public string? App2LoadState { get; set; }
        public string? App2State { get; set; }
        public string? AddressTableLoadState { get; set; }
        public string? AssociationTableLoadState { get; set; }
        public string? ObjectTableLoadState { get; set; }

        // ETS6 "Allgemein" / "Sicherheit"
        public string? BusVoltage { get; set; }          // PID_BUS_VOLTAGE (30), z.B. "30,0V"
        public string? SecurityMode { get; set; }        // PID_SECURITY_MODE (31), "Ein"/"Aus"
        public string? SecurityFailureCounters { get; set; }  // PID_SECURITY_FAILURE_COUNTERS (32)
        public string? SecurityLastFailure { get; set; }      // PID_SECURITY_LAST_FAILURE (33)
    }
}