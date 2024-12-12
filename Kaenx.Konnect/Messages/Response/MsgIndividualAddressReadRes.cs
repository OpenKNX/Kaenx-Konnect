using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Messages.Response
{
    public class MsgIndividualAddressReadRes : IMessageResponse
    {
        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = false;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress? SourceAddress { get; set; }
        public IKnxAddress? DestinationAddress { get; set; }
        public ApciTypes ApciType { get; set; } = ApciTypes.IndividualAddressResponse;
        public byte[] Raw { get; set; } = new byte[0];


        public byte[] GetBytesCemi()
        {
            //Nothing to do here
            return new byte[0];
        }

        public byte[] GetBytesEmi1()
        {
            //Nothing to do here
            return new byte[0];
        }

        public byte[] GetBytesEmi2()
        {
            //Nothing to do here
            return new byte[0];
        }

        public void ParseDataCemi() { }

        public void ParseDataEmi1() { }

        public void ParseDataEmi2() { }
    }
}
