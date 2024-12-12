using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    /// <summary>
    /// Creates a telegram to request the state of a function property
    /// </summary>
    public class MsgFunctionPropertyStateReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = true;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress? SourceAddress { get; set; }
        public IKnxAddress? DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.FunctionPropertyStateRead;
        public byte[] Raw { get; set; } = new byte[0];


        public byte ObjectIndex { get; set; } = 0;
        public byte PropertyId { get; set; } = 0;
        public byte[] Data { get; set; } = new byte[0];


        /// <summary>
        /// Creates a telegram to write to a property
        /// </summary>
        /// <param name="objectIndex">Object Index</param>
        /// <param name="propertyId">Property Id</param>
        /// <param name="data">Data to write</param>
        /// <param name="address">Unicast Address from the device</param>
        public MsgFunctionPropertyStateReq(byte objectIndex, byte propertyId, byte[] data, UnicastAddress address)
        {
            ObjectIndex = objectIndex;
            PropertyId = propertyId;
            DestinationAddress = address;
            Data = data;
        }

        public MsgFunctionPropertyStateReq() { }




        public byte[] GetBytesCemi()
        {
            TunnelRequest builder = new TunnelRequest();
            List<byte> data = new List<byte>() { ObjectIndex, PropertyId };
            data.AddRange(Data);

            builder.Build(SourceAddress, DestinationAddress, ApciType, SequenceNumber, data.ToArray());
            
            data = new List<byte>() { 0x11, 0x00 };
            data.AddRange(builder.GetBytes());
            return data.ToArray();
        }

        public byte[] GetBytesEmi1()
        {
            throw new NotImplementedException("GetBytesEmi1 - MsgPropertyWriteReq");
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException("GetBytesEmi2 - MsgPropertyWriteReq");
        }


        public void ParseDataCemi()
        {
            if(Raw.Length < 2)
                throw new Exception("Invalid raw length");

            ObjectIndex = Raw[0];
            PropertyId = Raw[1];
            Data = Raw.Skip(2).ToArray();
        }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgPropertyWriteReq");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgPropertyWriteReq");
        }
    }
}
