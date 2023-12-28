﻿using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Messages;
using Kaenx.Konnect.Parser;
using Kaenx.Konnect.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Responses
{
    class RoutingResponse : IParserMessage
    {
        public RoutingResponse(byte headerLength, byte protocolVersion, ushort totalLength, byte messageCode, byte addInformationLength, bool isNumbered, bool ackWanted, byte controlField,
          byte controlField2, UnicastAddress sourceAddress, IKnxAddress destinationAddress, ApciTypes apci, int seqNumb,
          byte[] data)
        {
            HeaderLength = headerLength;
            ProtocolVersion = protocolVersion;
            TotalLength = totalLength;
            MessageCode = messageCode;
            AddInformationLength = addInformationLength;
            AckWanted = ackWanted;
            ControlField = controlField;
            ControlField2 = controlField2;
            SourceAddress = sourceAddress;
            DestinationAddress = destinationAddress;
            APCI = apci;
            SequenceNumber = seqNumb;
            Data = data;
            IsNumbered = isNumbered;
        }

        public bool IsNumbered { get; }
        public byte HeaderLength { get; }
        public byte ProtocolVersion { get; }
        public ushort TotalLength { get; }
        public int SequenceNumber { get; }
        public byte MessageCode { get; }
        public byte AddInformationLength { get; }
        public bool AckWanted { get; }
        public byte ControlField { get; }
        public byte ControlField2 { get; }
        public UnicastAddress SourceAddress { get; }
        public IKnxAddress DestinationAddress { get; }
        public ApciTypes APCI { get; }
        public byte[] Data { get; }
    }
}