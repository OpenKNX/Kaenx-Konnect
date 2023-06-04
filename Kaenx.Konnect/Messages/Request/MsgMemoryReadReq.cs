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
    /// Creates a telegram to read from memory
    /// </summary>
    public class MsgMemoryReadReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = true;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.MemoryRead;
        public byte[] Raw { get; set; }
        public bool IsExtended { get; set; }


        public int Address { get; set; }
        public int Length { get; set; }


        /// <summary>
        /// Creates a telegram to read from memory
        /// </summary>
        /// <param name="address">Memory Address</param>
        /// <param name="length">Length of data to read</param>
        /// <param name="addr">Unicast Address from device</param>
        public MsgMemoryReadReq(int address, int length, UnicastAddress addr, bool isExtended = false)
        {
            Address = address;
            Length = length;
            DestinationAddress = addr;
            IsExtended = isExtended;
        }

        public MsgMemoryReadReq() { }


        public byte[] GetBytesEmi1()
        {
            if (IsExtended && Length > 256)
                throw new Exception("Bei Emi1 kann maximal 256 Bytes ausgelesen werden. (Angefordert waren " + Length + " bytes)[ExtendedFrame]");

            if (!IsExtended && Length > 13)
                throw new Exception("Bei Emi1 kann maximal 13 Bytes ausgelesen werden. (Angefordert waren " + Length + " bytes)[StandardFrame]");

            List<byte> bytes = new List<byte>();

            bytes.Add(0x4c);
            bytes.Add(BitConverter.GetBytes(Length)[1]);
            bytes.AddRange(BitConverter.GetBytes(Address));

            return bytes.ToArray();
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException("GetBytesEmi2 - MsgMemoryReadReq");
        }

        public byte[] GetBytesCemi()
        {
            if (IsExtended && Length > 256)
                throw new Exception("Bei cEmi kann maximal 256 Bytes ausgelesen werden. (Angefordert waren " + Length + " bytes)[ExtendedFrame]");

            if (!IsExtended && Length > 13)
                throw new Exception("Bei cEmi kann maximal 13 Bytes ausgelesen werden. (Angefordert waren " + Length + " bytes)[StandardFrame]");


            List<byte> data = new List<byte> { BitConverter.GetBytes(Length)[0] };
            byte[] addr = BitConverter.GetBytes(Address);
            data.Add(addr[1]);
            data.Add(addr[0]);

            Builders.TunnelRequest builder = new TunnelRequest();
            if(IsExtended) builder.SetIsExtended();
            builder.Build(SourceAddress, DestinationAddress, ApciTypes.MemoryRead, SequenceNumber, data.ToArray());
            data = new List<byte>() { 0x11, 0x00 };
            data.AddRange(builder.GetBytes());
            return data.ToArray();
        }


        public void ParseDataCemi()
        {
            Length = BitConverter.ToInt32(new byte[] { Raw[0], 0x00, 0x00, 0x00 }, 0);
            Address = BitConverter.ToInt32(new byte[] { Raw[2], Raw[1], 0x00, 0x00 }, 0);
        }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgMemoryreadReq");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - msgMemoryReadReq");
        }
    }
}
