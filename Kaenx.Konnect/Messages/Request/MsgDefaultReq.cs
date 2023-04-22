using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    public class MsgDefaultReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = true;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; set; }
        public ApciTypes ApciType { get; set; }
        public byte[] Raw { get; set; }


        public MsgDefaultReq() { }
        public MsgDefaultReq(bool isNumbered) => IsNumbered = isNumbered;


        public byte[] GetBytesCemi()
        {
            throw new NotImplementedException();
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
