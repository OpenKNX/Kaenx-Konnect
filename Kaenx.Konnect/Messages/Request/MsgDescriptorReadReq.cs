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
    /// Creates a telegram to read the device descriptor
    /// </summary>
    public class MsgDescriptorReadReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = true;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.DeviceDescriptorRead;
        public byte[] Raw { get; set; }

        /// <summary>
        /// Creates a telegram to read the device descriptor
        /// </summary>
        /// <param name="address">Unicast Address from device</param>
        public MsgDescriptorReadReq(UnicastAddress address)
        {
            DestinationAddress = address;
        }

        public MsgDescriptorReadReq() { }


        public byte[] GetBytesCemi()
        {
            List<byte> data = new List<byte>() { 0x11, 0x00 };
            TunnelRequest builder = new TunnelRequest();
            builder.Build(SourceAddress, DestinationAddress, ApciTypes.DeviceDescriptorRead, SequenceNumber);
            data.AddRange(builder.GetBytes());
            return data.ToArray();
        }

        public byte[] GetBytesEmi1()
        {
            List<byte> data = new List<byte>()
            {
                0x11, //L_Data.req
                0xb0, //Control Byte
                0x00, 0x00 //Source Address
            };

            data.AddRange(DestinationAddress.GetBytes());
            data.AddRange(new byte[]
            {
                0x61, //NPCI Byte
                0x43, 0x00 //NPDU+APCI
            });
            return data.ToArray();
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException("GetBytesEmi2 - MsgDescriptorReadReq");
        }




        public void ParseDataCemi() {
            //No Data to parse
         }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgDescriptorReadReq");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgDescriptorReadReq");
        }
    }
}
