using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    /// <summary>
    /// Creates a telegram to connect to a device
    /// </summary>
    public class MsgConnect : IMessageRequest
    {
        private byte _channelId;
        private int _sequenzeNumb;
        private byte _sequenzeCount;
        private UnicastAddress _address;

        /// <summary>
        /// Creates a telegram to connect to a device
        /// </summary>
        /// <param name="address">Unicast Address from device</param>
        public MsgConnect(UnicastAddress address)
        {
            _address = address;
        }

        public byte[] GetBytesCemi()
        {
            TunnelRequest builder = new TunnelRequest();
            builder.Build(UnicastAddress.FromString("0.0.0"), _address, Parser.ApciTypes.Connect, 255);
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
