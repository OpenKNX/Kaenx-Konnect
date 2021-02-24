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
    public class MsgMemoryWriteReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.MemoryWrite;
        public byte[] Raw { get; set; }


        private int Length { get; set; }
        private int Address { get; set; }
        private byte[] Data { get; set; }

        /// <summary>
        /// Creates a telegram to write to memory
        /// </summary>
        /// <param name="address">Memory Address</param>
        /// <param name="data">Data to write</param>
        /// <param name="uniAddr">Unicast Address from Device</param>
        /// <exception cref="Exception">Thrown if data length is greater than 256 bytes</exception>
        public MsgMemoryWriteReq(int address, byte[] data, UnicastAddress uniAddr)
        {
            if (data.Length > 256)
                throw new Exception("Es können maximal 256 Bytes geschrieben werden. (Angefordert waren " + data.Length + " bytes)");

            Address = address;
            Data = data;
            DestinationAddress = uniAddr;
        }

        public MsgMemoryWriteReq() { }




        public byte[] GetBytesCemi()
        {
            TunnelCemiRequest builder = new TunnelCemiRequest();
            List<byte> data = new List<byte> { Convert.ToByte(Data.Length) };
            byte[] addr = BitConverter.GetBytes(Convert.ToInt16(Address));
            Array.Reverse(addr);
            data.AddRange(addr);
            data.AddRange(Data);

            builder.Build(UnicastAddress.FromString("0.0.0"), DestinationAddress, ApciTypes.MemoryWrite, SequenceNumber, data.ToArray());
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




        public void ParseDataCemi()
        {
            Length = BitConverter.ToInt32(new byte[] { Raw[0], 0x00, 0x00, 0x00 }, 0);
            Address = BitConverter.ToInt32(new byte[] { Raw[2], Raw[1], 0x00, 0x00 }, 0);
            Data = Raw.Skip(2).ToArray();
        }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgMemoryWriteReq");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgMemoryWriteReq");
        }
    }
}
