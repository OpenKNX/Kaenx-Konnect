using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Enums
{
    public enum DibTypes
    {
        Unknown = 0,
        DeviceInfo = 0x01,
        SuppSvcFamilies = 0x02,
        IpConfig = 0x03,
        IpCurrentConfig = 0x04,
        KnxAddresses = 0x05,
        ManufacturerData = 0xFE
    }
}
