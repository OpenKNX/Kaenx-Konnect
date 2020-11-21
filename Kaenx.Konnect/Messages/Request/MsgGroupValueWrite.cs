using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    /// <summary>
    /// Creates a telegram to write a group value
    /// </summary>
    public class MsgGroupValueWrite : IMessageRequest
    {
        private MulticastAddress _addressTo { get; set; }
        private UnicastAddress _addressFrom { get; set; }
        private int _sequenzeNumb;
        private byte _channelId;
        private byte _sequenzeCount;
        private byte[] _data;

        /// <summary>
        /// Creates a telegram to write a group value
        /// </summary>
        /// <param name="from">Unicast Address from sender</param>
        /// <param name="to">Mulicast Address (GroupAddress)</param>
        /// <param name="data">Data to write</param>
        public MsgGroupValueWrite(UnicastAddress from, MulticastAddress to, byte[] data)
        {
            _addressFrom = from;
            _addressTo = to;
            _data = data;
        }

        public byte[] GetBytesCemi()
        {
            TunnelRequest builder = new TunnelRequest();
            builder.Build(_addressFrom, _addressTo, Parser.ApciTypes.GroupValueWrite, 255, _data);
            builder.SetChannelId(_channelId);
            builder.SetSequence(_sequenzeCount);
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

        public void SetInfo(byte channel, byte seqCounter)
        {
            _channelId = channel;
            _sequenzeCount = seqCounter;
        }

        public void SetSequenzeNumb(int seq)
        {
            _sequenzeNumb = seq;
        }
    }
}
