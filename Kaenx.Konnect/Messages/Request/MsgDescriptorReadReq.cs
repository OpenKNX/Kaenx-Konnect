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
            TunnelRequest builder = new TunnelRequest();
            builder.Build(UnicastAddress.FromString("0.0.0"), DestinationAddress, ApciTypes.DeviceDescriptorRead, SequenceNumber);
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




        public void ParseDataCemi() { }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgDescriptorRead");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgDescriptorRead");
        }
    }
}
