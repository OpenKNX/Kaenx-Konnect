using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Enums
{
    public enum ConnectionTypes
    {
        DeviceManagement = 0x03,
        Tunneling = 0x04,
        RemoteLogging = 0x06,
        RemoteConfiguration = 0x07,
        ObjectServer = 0x08
    }
}
