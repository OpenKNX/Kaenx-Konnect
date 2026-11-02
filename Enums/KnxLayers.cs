using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Enums
{
    public enum KnxLayers
    {
        Unknown = 0x00,
        LinkLayer = 0x02,
        Raw = 0x04,
        BusMonitor = 0x80
    }
}
