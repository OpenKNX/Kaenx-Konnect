using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Messages.Response
{
    public class MsgKeyWriteRes : IMessageResponse
    {
        public byte ChannelId { get; set; }
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.KeyResponse;
        public byte[] Raw { get; set; }


        public byte Level { get; set; }

        public MsgKeyWriteRes(byte level)
        {
            Level = level;
        }

        public MsgKeyWriteRes() { }


        public void ParseDataCemi()
        {
            Level = Raw[0];
        }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgSearchRes");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgSearchRes");
        }

        public byte[] GetBytesEmi1()
        {
            throw new NotImplementedException("GetBytesEmi1 - MsgSearchRes");
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException("GetBytesEmi2 - MsgSearchRes");
        }

        public byte[] GetBytesCemi()
        {
            throw new NotImplementedException("GetBytesCemi - MsgSearchRes");
        }
    }
}
