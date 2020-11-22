using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    /// <summary>
    /// Creates a telegram to write to memory
    /// </summary>
    public class MsgMemoryWrite : IMessageRequest
    {
        private int Address { get; set; }
        private byte[] Data { get; set; }

        private UnicastAddress _address { get; set; }
        private int _sequenzeNumb;
        private byte _channelId;
        private byte _sequenzeCount;

        /// <summary>
        /// Creates a telegram to write to memory
        /// </summary>
        /// <param name="address">Memory Address</param>
        /// <param name="data">Data to write</param>
        /// <param name="uniAddr">Unicast Address from Device</param>
        /// <exception cref="Exception">Thrown if data length is greater than 256 bytes</exception>
        public MsgMemoryWrite(int address, byte[] data, UnicastAddress uniAddr)
        {
            if (data.Length > 256)
                throw new Exception("Es können maximal 256 Bytes geschrieben werden. (Angefordert waren " + data.Length + " bytes)");

            Address = address;
            Data = data;
            _address = uniAddr;
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



        public byte[] GetBytesCemi()
        {
            TunnelRequest builder = new TunnelRequest();
            List<byte> data = new List<byte> { Convert.ToByte(Data.Length) };
            byte[] addr = BitConverter.GetBytes(Convert.ToInt16(Address));
            Array.Reverse(addr);
            data.AddRange(addr);
            data.AddRange(Data);

            builder.Build(UnicastAddress.FromString("0.0.0"), _address, ApciTypes.MemoryWrite, _sequenzeNumb, data.ToArray());
            builder.SetChannelId(_channelId);
            builder.SetSequence(_sequenzeCount);
            return builder.GetBytes();
        }

        public byte[] GetBytesEmi1()
        {

            List<byte> bytes = new List<byte>();

            bytes.Add(0x46);
            bytes.Add(BitConverter.GetBytes(Data.Length)[1]); //TODO check if Index 0 is used
            bytes.AddRange(BitConverter.GetBytes(Address));
            bytes.AddRange(Data);

            return bytes.ToArray();
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException();
        }

        public void SetEndpoint(IPEndPoint endpoint) { }
    }
}
