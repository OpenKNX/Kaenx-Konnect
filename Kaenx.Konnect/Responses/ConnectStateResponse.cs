﻿using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Responses
{
    class ConnectStateResponse : IResponse
    {
        public ConnectStateResponse(byte headerLength, byte protocolVersion, ushort totalLength, byte communicationChannel, byte status)
        {
            HeaderLength = headerLength;
            ProtocolVersion = protocolVersion;
            TotalLength = totalLength;
            CommunicationChannel = communicationChannel;
            Status = status;
        }

        public byte HeaderLength { get; }
        public byte ProtocolVersion { get; }
        public ushort TotalLength { get; }
        public byte CommunicationChannel { get; }
        public byte Status { get; }
    }
}
