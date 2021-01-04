using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Messages
{
    public interface IMessage
    {
        public byte ChannelId { get; set; }
        IKnxAddress SourceAddress { get; set; }
        IKnxAddress DestinationAddress { get; set; }
        byte SequenceCounter { get; set; }
        int SequenceNumber { get; set; }
        ApciTypes ApciType { get; }
        byte[] Raw { get; set; }



        byte[] GetBytesEmi1();
        byte[] GetBytesEmi2();
        byte[] GetBytesCemi();


        void ParseDataCemi();
        void ParseDataEmi1();
        void ParseDataEmi2();
    }
}
