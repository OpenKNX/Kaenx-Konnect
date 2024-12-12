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
    /// Creates a telegram to write a group value
    /// </summary>
    public class MsgGroupWriteReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = false;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress? SourceAddress { get; set; }
        public IKnxAddress? DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.GroupValueWrite;
        public byte[] Raw { get; set; } = new byte[0];

        private byte[] _data = new byte[0];

        /// <summary>
        /// Creates a telegram to write a group value
        /// </summary>
        /// <param name="from">Unicast Address from sender</param>
        /// <param name="to">Mulicast Address (GroupAddress)</param>
        /// <param name="data">Data to write</param>
        public MsgGroupWriteReq(UnicastAddress from, MulticastAddress to, byte[] data)
        {
            SourceAddress = from;
            DestinationAddress = to;
            _data = data;
        }

        public MsgGroupWriteReq() { }



        public byte[] GetBytesCemi()
        {
            List<byte> data = new List<byte>() { 0x11, 0x00 };
            TunnelRequest builder = new TunnelRequest();
            builder.Build(SourceAddress, DestinationAddress, ApciTypes.GroupValueWrite, 255, _data);
            data.AddRange(builder.GetBytes());
            return data.ToArray();
        }

        public byte[] GetBytesEmi1()
        {
            throw new NotImplementedException("GetBytesEmi1 - MsgGroupValueWriteReq");
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException("GetBytesEmi2 - MsgGroupValueWriteReq");
        }


        public void ParseDataCemi() { } //No Data to parse, only Raw

        public void ParseDataEmi1() { } //No Data to parse, only Raw

        public void ParseDataEmi2() { } //No Data to parse, only Raw
    }
}
