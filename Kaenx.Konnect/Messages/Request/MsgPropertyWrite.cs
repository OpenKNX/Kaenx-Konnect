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
    /// Creates a telegram to write to a property
    /// </summary>
    public class MsgPropertyWrite : IMessageRequest
    {
        public byte ObjectIndex { get; set; }
        public byte PropertyId { get; set; }
        public byte[] Data { get; set; }

        private byte _channelId;
        private int _sequenzeNumb;
        private byte _sequenzeCount;
        private UnicastAddress _address;

        /// <summary>
        /// Creates a telegram to write to a property
        /// </summary>
        /// <param name="objectIndex">Object Index</param>
        /// <param name="propertyId">Property Id</param>
        /// <param name="data">Data to write</param>
        /// <param name="address">Unicast Address from the device</param>
        public MsgPropertyWrite(byte objectIndex, byte propertyId, byte[] data, UnicastAddress address)
        {
            ObjectIndex = objectIndex;
            PropertyId = propertyId;
            _address = address;
            Data = data;
        }


        public byte[] GetBytesCemi()
        {
            byte[] send_data = new byte[Data.Length + 4];

            send_data[0] = ObjectIndex;
            send_data[1] = PropertyId;
            send_data[2] = 0x10;
            send_data[3] = 0x01;

            for (int i = 0; i < Data.Length; i++)
                send_data[i + 4] = Data[i];

            TunnelRequest builder = new TunnelRequest();
            builder.Build(UnicastAddress.FromString("0.0.0"), _address, ApciTypes.PropertyValueWrite, _sequenzeNumb, send_data);
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
