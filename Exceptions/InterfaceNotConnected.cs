using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Exceptions
{
    public class InterfaceNotConnectedException : Exception
    {
        public InterfaceNotConnectedException() : base("The interface is not connected.") { }
        public InterfaceNotConnectedException(string message) : base(message) { }
        public InterfaceNotConnectedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
