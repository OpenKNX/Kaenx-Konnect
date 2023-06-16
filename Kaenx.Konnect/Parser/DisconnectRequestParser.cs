using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Responses;
using Kaenx.Konnect.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Parser
{
    class DisconnectRequestParser : IReceiveParser
    {
        public ushort ServiceTypeIdentifier => 0x0209;

        IParserMessage IReceiveParser.Build(byte headerLength, byte protocolVersion, ushort totalLength,
          byte[] responseBytes)
        {
            return Build(headerLength, protocolVersion, totalLength, responseBytes);
        }

        public DisconnectRequest Build(byte headerLength, byte protocolVersion, ushort totalLength, byte[] responseBytes)
        {
            var communicationChannel = responseBytes[0];
            var status = responseBytes[1];

            return new DisconnectRequest(headerLength, protocolVersion, totalLength, communicationChannel, status);
        }
    }
}
