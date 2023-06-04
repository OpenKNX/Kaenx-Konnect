using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Messages;
using Kaenx.Konnect.Responses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Kaenx.Konnect.Parser
{
    class RoutingParser : IReceiveParser
    {
        public ushort ServiceTypeIdentifier => 0x0530;

        public RoutingParser() { }

        IParserMessage IReceiveParser.Build(byte headerLength, byte protocolVersion, ushort totalLength,
          byte[] responseBytes)
        {
            return Build(headerLength, protocolVersion, totalLength, responseBytes);
        }


        public Builders.RoutingResponse Build(byte headerLength, byte protocolVersion, ushort totalLength, byte[] responseBytes)
        {
            var x = new TunnelRequestParser();
            Builders.TunnelResponse resp = x.Build(headerLength, protocolVersion, totalLength, responseBytes);
            return new RoutingResponse(resp.HeaderLength,
                resp.ProtocolVersion,
                resp.TotalLength,
                resp.StructureLength,
                resp.CommunicationChannel,
                resp.SequenceCounter,
                resp.MessageCode,
                resp.AddInformationLength,
                resp.IsNumbered,
                resp.AckWanted,
                resp.ControlField,
                resp.ControlField2,
                resp.SourceAddress,
                resp.DestinationAddress,
                resp.APCI,
                resp.SequenceNumber,
                resp.Data);
        }
    }
}
