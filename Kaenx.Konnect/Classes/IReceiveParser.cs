using Kaenx.Konnect.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Classes
{
    interface IReceiveParser
    {
        ushort ServiceTypeIdentifier { get; }
        IParserMessage Build(byte headerLength, byte protocolVersion, ushort totalLength, byte[] responseBytes);
    }
}
