using Kaenx.Konnect.Addresses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Messages.Response
{
    public class MsgGroupReadRes : IMessageResponse
    {
        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = false;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress? SourceAddress { get; set; }
        public IKnxAddress? DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.GroupValueResponse;
        public byte[] Raw { get; set; } = new byte[0];



        public MsgGroupReadRes() { }

        public MsgGroupReadRes(byte[] data)
        {
            Raw = data;
        }



        public void ParseDataCemi() { }

        public void ParseDataEmi1() { }

        public void ParseDataEmi2() { }

        public byte[] GetBytesEmi1()
        {
            return Raw;
        }

        public byte[] GetBytesEmi2()
        {
            return Raw;
        }

        public byte[] GetBytesCemi()
        {
            return Raw;
        }
    }
}
