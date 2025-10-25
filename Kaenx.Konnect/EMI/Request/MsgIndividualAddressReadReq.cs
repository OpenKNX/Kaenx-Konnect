using Kaenx.Konnect.Addresses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    /// <summary>
    /// Creates a telegram to read individual address from devices in programm mode
    /// </summary>
    public class MsgIndividualAddressReadReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = false;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress? SourceAddress { get; set; }
        public IKnxAddress? DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.IndividualAddressRead;
        public byte[] Raw { get; set; } = new byte[0];


        public byte[] GetBytesCemi()
        {
            List<byte> data = new List<byte>() { 0x11, 0x00 };
            //TunnelRequest builder = new TunnelRequest();
            //builder.Build(SourceAddress, MulticastAddress.FromString("0/0/0"), ApciTypes.IndividualAddressRead);
            //data.AddRange(builder.GetBytes());
            return data.ToArray();
        }

        public byte[] GetBytesEmi1()
        {
            List<byte> data = new List<byte>() { 0x11, 0x00 };
            //TunnelRequest builder = new TunnelRequest();
            //builder.Build(SourceAddress, MulticastAddress.FromString("0/0/0"), ApciTypes.IndividualAddressRead);
            //data.AddRange(builder.GetBytes());
            return data.ToArray();
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException("GetBytesEmi2 - MsgIndividualAddressReadReq");
        }


        public void ParseDataCemi() { } //No Data to parse

        public void ParseDataEmi1() { } //No Data to parse

        public void ParseDataEmi2() { } //No Data to parse
    }
}
