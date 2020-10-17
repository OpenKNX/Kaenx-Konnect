using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    /// <summary>
    /// Creates a telegram to write an individual address via serialnumber
    /// </summary>
    public class MsgIndividualAddressSerialWrite : IMessageRequest
    {
        private UnicastAddress _address { get; set; }
        private byte[] _serial;
        private int _sequenzeNumb;
        private byte _channelId;
        private byte _sequenzeCount;

        /// <summary>
        /// Creates a telegram to write an individual address via serialnumber
        /// </summary>
        /// <param name="address">New Unicast Address</param>
        /// <param name="serial">Serialnumber</param>
        public MsgIndividualAddressSerialWrite(UnicastAddress newAddr, byte[] serial)
        {
            _address = newAddr;
            _serial = serial;
        }

        public byte[] GetBytesCemi()
        {
            TunnelRequest builder = new TunnelRequest();

            List<byte> data = new List<byte>();
            data.AddRange(_serial);

            data.AddRange(_address.GetBytes());
            data.AddRange(new byte[] { 0, 0, 0, 0 });

            builder.Build(MulticastAddress.FromString("0/0/0"), MulticastAddress.FromString("0/0/0"), Parser.ApciTypes.IndividualAddressSerialNumberWrite, 255, data.ToArray());
            builder.SetPriority(Prios.System);
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
