using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    /// <summary>
    /// Creates a telegram to read a property
    /// </summary>
    public class MsgPropertyRead : IMessageRequest
    {
        public byte ObjectIndex { get; set; }
        public byte PropertyId { get; set; }

        private byte _channelId;
        private int _sequenzeNumb;
        private byte _sequenzeCount;
        private UnicastAddress _address;

        /// <summary>
        /// Creates a telegram to read a property
        /// </summary>
        /// <param name="objIndex">Object Index</param>
        /// <param name="propId">Property Id</param>
        /// <param name="address">Unicast Address from device</param>
        public MsgPropertyRead(byte objIndex, byte propId, UnicastAddress address)
        {
            ObjectIndex = objIndex;
            PropertyId = propId;
            _address = address;
        }

        public byte[] GetBytesCemi()
        {
            TunnelRequest builder = new TunnelRequest();

            byte[] data = { ObjectIndex, PropertyId, 0x10, 0x01 };

            builder.Build(UnicastAddress.FromString("0.0.0"), _address, ApciTypes.PropertyValueRead, _sequenzeNumb, data);
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
