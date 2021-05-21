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
    /// Creates a telegram to connect to a device
    /// </summary>
 
    public class MsgConnectReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.Connect;
        public byte[] Raw { get; set; }



        /// <summary>
        /// Creates a telegram to connect to a device
        /// </summary>
        /// <param name="address">Unicast Address from device</param>
        public MsgConnectReq(UnicastAddress address)
        {
            DestinationAddress = address;
        }

        public MsgConnectReq() { }


        public byte[] GetBytesCemi()
        {
            List<byte> data = new List<byte>() { 0x11, 0x00 };
            TunnelRequest builder = new TunnelRequest();
            builder.Build(UnicastAddress.FromString("0.0.0"), DestinationAddress, ApciTypes.Connect, 255);
            data.AddRange(builder.GetBytes());
            return data.ToArray();
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

        public void ParseDataEmi1() { }

        public void ParseDataEmi2() { }
    }
}
