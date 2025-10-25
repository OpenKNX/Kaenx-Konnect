using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Messages.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Messages.Response
{
    public class MsgReadSerialNumberRes : IMessageResponse
    {
        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = false;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress? SourceAddress { get; set; }
        public IKnxAddress? DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.SystemNetworkParameterResponse;
        public byte[] Raw { get; set; } = new byte[0];

        public MsgSystemNetworkParameterReadOperand Operand { get; set; }

        public byte[] TestResult = new byte[0];


        public MsgReadSerialNumberRes(byte[] data) => Raw = data;
        public MsgReadSerialNumberRes() { }


        public void ParseDataCemi()
        {
            Operand = Enum.Parse<MsgSystemNetworkParameterReadOperand>(((int)Raw[4]).ToString());
            TestResult = Raw.Skip(5).ToArray();
        }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgReadSerialNumberRes");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgReadSerialNumberRes");
        }


        public byte[] GetBytesEmi1()
        {
            throw new NotImplementedException("GetBytesEmi1 - MsgReadSerialNumberRes");
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException("GetBytesEmi2 - MsgReadSerialNumberRes");
        }

        public byte[] GetBytesCemi()
        {
            throw new NotImplementedException("GetBytesCemi - MsgReadSerialNumberRes");
        }
    }
}
