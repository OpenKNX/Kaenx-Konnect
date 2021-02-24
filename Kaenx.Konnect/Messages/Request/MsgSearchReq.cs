using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Parser;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    /// <summary>
    /// Creates a telegram to search for interfaces (only for IP and cEMI)
    /// </summary>
    public class MsgSearchReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.Undefined;
        public byte[] Raw { get; set; }



        public IPEndPoint Endpoint { get; set; }

        /// <summary>
        /// Creates a telegram to search for interfaces (only for IP and cEMI)
        /// </summary>
        public MsgSearchReq() { }

        public byte[] GetBytesCemi()
        {
            List<byte> bytes = new List<byte>() { 0x06, 0x10, 0x02, 0x01, 0x00, 0x0e }; // Length, Version, Descriptor 2x, Total length 2x
            bytes.AddRange(new HostProtocolAddressInformation(0x01, Endpoint).GetBytes());
            return bytes.ToArray();
        }

        public byte[] GetBytesEmi1()
        {
            throw new NotImplementedException();
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException();
        }


        public void ParseDataCemi()
        {
            throw new NotImplementedException("ParseDataCemi - MsgSearchReq");
        }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgSearchReq");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgSearchReq");
        }
    }
}
