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
    /// Creates a telegram to write to a property
    /// </summary>
    public class MsgPropertyWriteReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = true;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.PropertyValueWrite;
        public byte[] Raw { get; set; }


        public byte ObjectIndex { get; set; }
        public byte PropertyId { get; set; }
        public byte[] Data { get; set; }


        /// <summary>
        /// Creates a telegram to write to a property
        /// </summary>
        /// <param name="objectIndex">Object Index</param>
        /// <param name="propertyId">Property Id</param>
        /// <param name="data">Data to write</param>
        /// <param name="address">Unicast Address from the device</param>
        public MsgPropertyWriteReq(byte objectIndex, byte propertyId, byte[] data, UnicastAddress address)
        {
            ObjectIndex = objectIndex;
            PropertyId = propertyId;
            DestinationAddress = address;
            Data = data;
        }

        public MsgPropertyWriteReq() { }




        public byte[] GetBytesCemi()
        {
            byte[] send_data = new byte[Data.Length + 4];

            send_data[0] = ObjectIndex;
            send_data[1] = PropertyId;
            send_data[2] = 0x10; //TODO check if it must be set to data.length
            send_data[3] = 0x01;

            for (int i = 0; i < Data.Length; i++)
                send_data[i + 4] = Data[i];

            List<byte> data = new List<byte>() { 0x11, 0x00 };
            TunnelRequest builder = new TunnelRequest();
            builder.Build(UnicastAddress.FromString("0.0.0"), DestinationAddress, ApciTypes.PropertyValueWrite, SequenceNumber, send_data);
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
