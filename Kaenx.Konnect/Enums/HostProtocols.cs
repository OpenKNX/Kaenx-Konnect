using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Enums
{
    public enum HostProtocols
    {
        Unknown = 0,
        IPv4_UDP = 0x01,
        IPv4_TCP = 0x02,
        IPv6_UDP = 0x03,
        IPv6_TCP = 0x04,
    }
}
