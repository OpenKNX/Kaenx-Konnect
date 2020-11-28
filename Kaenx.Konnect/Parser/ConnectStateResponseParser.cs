using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Parser
{
    //TODO: Make private
    public class ConnectStateResponseParser : IReceiveParser
    {
        public ushort ServiceTypeIdentifier => 0x0208;

        IResponse IReceiveParser.Build(byte headerLength, byte protocolVersion, ushort totalLength,
          byte[] responseBytes)
        {
            return Build(headerLength, protocolVersion, totalLength, responseBytes);
        }

        public ConnectStateResponse Build(byte headerLength, byte protocolVersion, ushort totalLength, byte[] responseBytes)
        {
            var communicationChannel = responseBytes[0];
            var status = responseBytes[1];

            return new ConnectStateResponse(headerLength, protocolVersion, totalLength, communicationChannel, status);
        }
    }
}
