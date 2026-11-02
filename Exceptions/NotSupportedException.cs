using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Exceptions
{
    public class NotSupportedException : Exception
    {
        public NotSupportedException() : base() { }
        public NotSupportedException(string message) : base(message) { }
        public NotSupportedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
