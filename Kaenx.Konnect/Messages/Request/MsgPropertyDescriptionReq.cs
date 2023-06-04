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
    public class MsgPropertyDescriptionReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = true;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.PropertyDescriptionRead;
        public byte[] Raw { get; set; }



        public byte ObjectIndex { get; set; }
        public byte PropertyId { get; set; }
        public byte PropertyIndex { get; set; }


        /// <summary>
        /// Creates a telegram to read a property
        /// </summary>
        /// <param name="objIndex">Object Index</param>
        /// <param name="propId">Property Id</param>
        /// <param name="address">Unicast Address from device</param>
        public MsgPropertyDescriptionReq(byte objIndex, byte propId, byte propIdx, UnicastAddress address)
        {
            ObjectIndex = objIndex;
            PropertyId = propId;
            PropertyIndex = propIdx;
            DestinationAddress = address;
        }

        public MsgPropertyDescriptionReq() { }



        public byte[] GetBytesCemi()
        {
            TunnelRequest builder = new TunnelRequest();
            List<byte> data = new List<byte>() { ObjectIndex, PropertyId, PropertyIndex };

            builder.Build(SourceAddress, DestinationAddress, ApciType, SequenceNumber, data.ToArray());
            data = new List<byte>() { 0x11, 0x00 };
            data.AddRange(builder.GetBytes());
            return data.ToArray();
        }

        public byte[] GetBytesEmi1()
        {
            throw new NotImplementedException("GetBytesEmi1 - MsgPropertyDescriptionReq");
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException("GetBytesEmi2 - MsgPropertyDescriptionReq");
        }


        public void ParseDataCemi()
        {
            ObjectIndex = Raw[0];
            PropertyId = Raw[1];
            PropertyIndex = Raw[2];
        }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgPropertyDescriptionReq");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgPropertyDescriptionReq");
        }
    }
}
