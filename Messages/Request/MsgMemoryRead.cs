using Kaenx.Konnect.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    /// <summary>
    /// Creates a telegram to read from memory
    /// </summary>
    public class MsgMemoryRead : IMessageRequest
    {
        public int Address { get; set; }
        public int Length { get; set; }

        private byte _channelId;
        private int _sequenzeNumb;
        private byte _sequenzeCount;
        private Addresses.UnicastAddress _address;

        /// <summary>
        /// Creates a telegram to read from memory
        /// </summary>
        /// <param name="address">Memory Address</param>
        /// <param name="length">Length of data to read</param>
        /// <param name="addr">Unicast Address from device</param>
        public MsgMemoryRead(int address, int length, Addresses.UnicastAddress addr)
        {
            Address = address;
            Length = length;
            _address = addr;
        }

        public void SetSequenzeNumb(int seq)
        {
            _sequenzeNumb = seq;
        }

        public void SetInfo(byte channel, byte seqCounter)
        {
            _channelId = channel;
            _sequenzeCount = seqCounter;
        }


        public byte[] GetBytesEmi1()
        {
            if (Length > 256)
                throw new Exception("Bei Emi1 kann maximal 256 Bytes ausgelesen werden. (Angefordert waren " + Length + " bytes)");

            List<byte> bytes = new List<byte>();

            bytes.Add(0x4c);
            bytes.Add(BitConverter.GetBytes(Length)[1]);
            bytes.AddRange(BitConverter.GetBytes(Address));

            return bytes.ToArray();
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException("Wird noch hinzugefügt");
        }

        public byte[] GetBytesCemi()
        {
            if (Length > 256)
                throw new Exception("Bei cEmi kann maximal 256 Bytes ausgelesen werden. (Angefordert waren " + Length + " bytes)");


            List<byte> data = new List<byte> { BitConverter.GetBytes(Length)[0] };
            byte[] addr = BitConverter.GetBytes(Address);
            data.Add(addr[1]);
            data.Add(addr[0]);

            Builders.TunnelRequest builder = new TunnelRequest();
            builder.Build(Addresses.UnicastAddress.FromString("0.0.0"), _address, Parser.ApciTypes.MemoryRead, _sequenzeNumb, data.ToArray());
            builder.SetChannelId(_channelId);
            builder.SetSequence(_sequenzeCount);

            return builder.GetBytes();
        }
    }
}
