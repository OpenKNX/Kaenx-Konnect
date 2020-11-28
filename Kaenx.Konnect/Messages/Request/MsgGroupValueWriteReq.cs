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
    public class MsgGroupValueWriteReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.GroupValueWrite;
        public byte[] Raw { get; set; }

        private byte[] _data;

        /// <summary>
        /// Creates a telegram to write a group value
        /// </summary>
        /// <param name="from">Unicast Address from sender</param>
        /// <param name="to">Mulicast Address (GroupAddress)</param>
        /// <param name="data">Data to write</param>
        public MsgGroupValueWriteReq(UnicastAddress from, MulticastAddress to, byte[] data)
        {
            SourceAddress = from;
            DestinationAddress = to;
            _data = data;
        }

        public MsgGroupValueWriteReq() { }



        public byte[] GetBytesCemi()
        {
            TunnelRequest builder = new TunnelRequest();
            builder.Build(SourceAddress, DestinationAddress, Parser.ApciTypes.GroupValueWrite, 255, _data);
            builder.SetChannelId(ChannelId);
            builder.SetSequence(SequenceCounter);
            return builder.GetBytes();
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
            throw new NotImplementedException("ParseDataCemi - MsgGroupValueWriteReq");
        }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgGroupValueWriteReq");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgGroupValueWriteReq");
        }
    }
}
