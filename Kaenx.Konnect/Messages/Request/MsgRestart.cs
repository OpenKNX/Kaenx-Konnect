using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    public class MsgRestart : IMessageRequest
    {
        private byte _channelId;
        private int _sequenzeNumb;
        private byte _sequenzeCount;
        private UnicastAddress _address;

        public MsgRestart(UnicastAddress address)
        {
            _address = address;
        }

        public byte[] GetBytesCemi()
        {
            TunnelRequest builder = new TunnelRequest();
            builder.Build(UnicastAddress.FromString("0.0.0"), _address, Parser.ApciTypes.Restart, _sequenzeNumb);
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

        public void SetEndpoint(IPEndPoint endpoint) { }
    }
}
