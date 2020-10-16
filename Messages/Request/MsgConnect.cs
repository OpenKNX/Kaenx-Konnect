using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    public class MsgConnect : IMessageRequest
    {
        private byte _channelId;
        private int _sequenzeNumb;
        private byte _sequenzeCount;
        private UnicastAddress _address;


        public MsgConnect(UnicastAddress address)
        {
            _address = address;
        }

        public byte[] GetBytesCemi()
        {
            TunnelRequest builder = new TunnelRequest();
            builder.Build(UnicastAddress.FromString("0.0.0"), _address, Parser.ApciTypes.Connect, 255);
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
            throw new NotImplementedException();
        }

        public void SetSequenzeNumb(int seq)
        {
            throw new NotImplementedException();
        }
    }
}
