using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Exceptions
{
    public class SequenceMissmatchException : Exception
    {
        public SequenceMissmatchException(string message) : base(message) { }
        public SequenceMissmatchException(Exception innerException) : base("The Sequence does not match.", innerException) { }
    }
}
