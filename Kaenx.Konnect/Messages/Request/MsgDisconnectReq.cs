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
    /// Creates a telegram to disconnect from device
    /// </summary>
    public class MsgDisconnectReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = true;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress? SourceAddress { get; set; }
        public IKnxAddress? DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.Disconnect;
        public byte[] Raw { get; set; } = new byte[0];

        /// <summary>
        /// Creates a telegram to disconnect from device
        /// </summary>
        /// <param name="address">Unicast Address from device</param>
        public MsgDisconnectReq(UnicastAddress address)
        {
            DestinationAddress = address;
        }

        public MsgDisconnectReq() { }



        public byte[] GetBytesCemi()
        {
            List<byte> data = new List<byte>() { 0x11, 0x00 };
            TunnelRequest builder = new TunnelRequest();
            builder.Build(SourceAddress, DestinationAddress, ApciTypes.Disconnect, 255);
            data.AddRange(builder.GetBytes());
            return data.ToArray();
        }

        public byte[] GetBytesEmi1()
        {
            throw new NotImplementedException("GetBytesEmi1 - MsgDisconnectReq");
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException("GetBytesEmi2 - MsgDisconnectReq");
        }



        public void ParseDataCemi() { } //No Data to parse

        public void ParseDataEmi1() { } //No Data to parse

        public void ParseDataEmi2() { } //No Data to parse
    }
}
