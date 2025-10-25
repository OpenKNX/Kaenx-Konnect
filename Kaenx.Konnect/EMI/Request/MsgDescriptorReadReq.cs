using Kaenx.Konnect.Addresses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    /// <summary>
    /// Creates a telegram to read the device descriptor
    /// </summary>
    public class MsgDeviceDescriptorReadReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = true;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress? SourceAddress { get; set; }
        public IKnxAddress? DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.DeviceDescriptorRead;
        public byte[] Raw { get; set; } = new byte[0];

        /// <summary>
        /// Creates a telegram to read the device descriptor
        /// </summary>
        /// <param name="address">Unicast Address from device</param>
        public MsgDeviceDescriptorReadReq(UnicastAddress address)
        {
            DestinationAddress = address;
        }

        public MsgDeviceDescriptorReadReq() { }


        public byte[] GetBytesCemi()
        {
            return new byte[1] { 0x00 }; // Type 0
        }

        public byte[] GetBytesEmi1()
        {
            if(DestinationAddress == null)
                throw new Exception("Destination Address is missing");
                
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
            throw new NotImplementedException("GetBytesEmi2 - MsgDeviceDescriptorReadReq");
        }




        public void ParseDataCemi() {
            //No Data to parse
         }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgDeviceDescriptorReadReq");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgDeviceDescriptorReadReq");
        }
    }
}
