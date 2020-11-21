using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    /// <summary>
    /// Creates a telegram to write an individual addres via programm button
    /// </summary>
    public class MsgIndividualAddressWrite : IMessageRequest
    {
        private UnicastAddress _address { get; set; }
        private int _sequenzeNumb;
        private byte _channelId;
        private byte _sequenzeCount;

        /// <summary>
        /// Creates a telegram to write an individual addres via programm button
        /// </summary>
        /// <param name="newAddress">New Unicast Address</param>
        public MsgIndividualAddressWrite(UnicastAddress newAddress)
        {
            _address = newAddress;
        }

        public byte[] GetBytesCemi()
        {
            TunnelRequest builder = new TunnelRequest();
            builder.Build(MulticastAddress.FromString("0/0/0"), MulticastAddress.FromString("0/0/0"), Parser.ApciTypes.IndividualAddressWrite, 255, _address.GetBytes());
            builder.SetPriority(Prios.System);
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
