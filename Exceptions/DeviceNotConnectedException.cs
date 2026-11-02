using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Exceptions
{
    public class DeviceNotConnectedException : Exception
    {
        public DeviceNotConnectedException() : base("The Device is not connected.") { }
        public DeviceNotConnectedException(Exception innerException) : base("The Device is not connected.", innerException) { }
    }
}
