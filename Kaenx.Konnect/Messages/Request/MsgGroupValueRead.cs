using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    /// <summary>
    /// Creates a telegram to read a group value
    /// </summary>
    public class MsgGroupValueRead : IMessageRequest
    {
        private MulticastAddress _address { get; set; }
        private int _sequenzeNumb;
        private byte _channelId;
        private byte _sequenzeCount;

        /// <summary>
        /// Creates a telegram to read a group value
        /// </summary>
        /// <param name="address">Multicast Address (GroupAddress)</param>
        public MsgGroupValueRead(MulticastAddress address)
        {
            _address = address;
        }

        public byte[] GetBytesCemi()
        {
            TunnelRequest builder = new TunnelRequest();
            builder.Build(UnicastAddress.FromString("0.0.0"), _address, Parser.ApciTypes.GroupValueRead);
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
            throw new NotImplementedException();
        }

        public void SetSequenzeNumb(int seq)
        {
            throw new NotImplementedException();
        }
    }
}
