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
        public bool IsNumbered { get; } = false;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress? SourceAddress { get; set; }
        public IKnxAddress? DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.Undefined;
        public byte[] Raw { get; set; } = new byte[0];


        public string IPAddress = "";


        public IPEndPoint? Endpoint { get; set; }

        /// <summary>
        /// Creates a telegram to search for interfaces (only for IP and cEMI)
        /// </summary>
        public MsgSearchReq() { }


        public MsgSearchReq(byte[] data) => Raw = data;

        public byte[] GetBytesCemi()
        {
            if(Endpoint == null)
                throw new Exception("Endpoint is required");
            List<byte> bytes = new List<byte>() { 0x06, 0x10, 0x02, 0x01, 0x00, 0x0e }; // Length, Version, Descriptor 2x, Total length 2x
            bytes.AddRange(new HostProtocolAddressInformation(0x01, Endpoint).GetBytes());
            return bytes.ToArray();
        }

        public byte[] GetBytesEmi1()
        {
            throw new NotImplementedException("GetBytesEmi1 - MsgSearchReq");
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException("GetBytesEmi2 - MsgSearchReq");
        }


        public void ParseDataCemi()
        {
            if(Raw.Length != 8)
                throw new Exception("Invalid Data Length");
                
            byte[] addr = new byte[4] { Raw[2], Raw[3], Raw[4], Raw[5] };
            Endpoint = new IPEndPoint(new IPAddress(addr), BitConverter.ToUInt16(new byte[2] { Raw[7], Raw[6] }, 0));
            IPAddress = new IPAddress(addr).ToString();
        }

        public void ParseDataEmi1() { } //No Data to parse

        public void ParseDataEmi2() { } //No Data to parse
    }
}
