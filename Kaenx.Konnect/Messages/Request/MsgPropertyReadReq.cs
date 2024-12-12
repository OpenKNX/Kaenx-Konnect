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
        public bool IsNumbered { get; } = true;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress? SourceAddress { get; set; }
        public IKnxAddress? DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.PropertyValueRead;
        public byte[] Raw { get; set; } = new byte[0];



        public byte ObjectIndex { get; set; } = 0;
        public byte PropertyId { get; set; } = 0;


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
            List<byte> data = new List<byte>() { ObjectIndex, PropertyId, 0x10, 0x01 };

            builder.Build(SourceAddress, DestinationAddress, ApciTypes.PropertyValueRead, SequenceNumber, data.ToArray());
            
            data = new List<byte>() { 0x11, 0x00 };
            data.AddRange(builder.GetBytes());
            return data.ToArray();
        }

        public byte[] GetBytesEmi1()
        {
            List<byte> data = new List<byte>() { ObjectIndex, PropertyId, 0x10, 0x01 };
            Emi2Request builder = new Emi2Request();
            builder.Build(DestinationAddress, ApciTypes.PropertyValueRead, SequenceNumber, data.ToArray());
            return builder.GetBytes();
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException("GetBytesEmi2 - MsgPropertyReadReq");
        }


        public void ParseDataCemi()
        {
            if(Raw.Length < 2)
                throw new Exception("Invalid raw Length");
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
