using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Exceptions
{
    public class InterfaceException : Exception
    {
        public InterfaceException(string message) : base(message) { }
        public InterfaceException(string message, Exception innerException) : base(message, innerException) { }
    }
}
