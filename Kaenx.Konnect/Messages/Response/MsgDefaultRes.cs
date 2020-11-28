using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Messages.Response
{
    public class MsgDefaultRes : IMessageResponse
    {
        public byte ChannelId { get; set; }
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; set; }
        public ApciTypes ApciType { get; set; }
        public byte[] Raw { get; set; }




        public void ParseDataCemi() { }

        public void ParseDataEmi1() { }

        public void ParseDataEmi2() { }
    }
}
