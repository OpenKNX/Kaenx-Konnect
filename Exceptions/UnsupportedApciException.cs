using System;
using Kaenx.Konnect.Enums;

namespace Kaenx.Konnect.Exceptions
{
    public class UnsupportedApciException : NotImplementedException
    {
        public ApciTypes ApciType { get; }

        public UnsupportedApciException(ApciTypes apciType)
            : base("No IDataMessage found for ApciType: " + apciType)
        {
            ApciType = apciType;
        }
    }
}
