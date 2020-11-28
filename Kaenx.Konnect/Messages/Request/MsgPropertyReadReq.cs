using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Parser;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    /// <summary>
    /// Creates a telegram to read a property
    /// </summary>
    public class MsgPropertyReadReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.PropertyValueRead;
        public byte[] Raw { get; set; }



        public byte ObjectIndex { get; set; }
        public byte PropertyId { get; set; }


        /// <summary>
        /// Creates a telegram to read a property
        /// </summary>
        /// <param name="objIndex">Object Index</param>
        /// <param name="propId">Property Id</param>
        /// <param name="address">Unicast Address from device</param>
        public MsgPropertyReadReq(byte objIndex, byte propId, UnicastAddress address)
        {
            ObjectIndex = objIndex;
            PropertyId = propId;
            DestinationAddress = address;
        }

        public MsgPropertyReadReq() { }



        public byte[] GetBytesCemi()
        {
            TunnelRequest builder = new TunnelRequest();

            byte[] data = { ObjectIndex, PropertyId, 0x10, 0x01 };

            builder.Build(UnicastAddress.FromString("0.0.0"), DestinationAddress, ApciTypes.PropertyValueRead, SequenceNumber, data);
            builder.SetChannelId(ChannelId);
            builder.SetSequence(SequenceCounter);
            return builder.GetBytes();
        }

        public byte[] GetBytesEmi1()
        {
            throw new NotImplementedException();
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException();
        }


        public void ParseDataCemi()
        {
            ObjectIndex = Raw[0];
            PropertyId = Raw[1];
        }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgPropertyReadReq");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgPropertyReadReq");
        }
    }
}
