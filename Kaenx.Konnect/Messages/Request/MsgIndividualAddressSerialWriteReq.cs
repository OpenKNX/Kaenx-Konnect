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
    /// Creates a telegram to write an individual address via serialnumber
    /// </summary>
    public class MsgIndividualAddressSerialWriteReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = false;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress? SourceAddress { get; set; }
        public IKnxAddress? DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.IndividualAddressSerialNumberWrite;
        public byte[] Raw { get; set; } = new byte[0];


        public UnicastAddress? NewAddress { get; set; }
        public byte[] Serial { get; set; } = new byte[0];

        /// <summary>
        /// Creates a telegram to write an individual address via serialnumber
        /// </summary>
        /// <param name="address">New Unicast Address</param>
        /// <param name="serial">Serialnumber</param>
        public MsgIndividualAddressSerialWriteReq(UnicastAddress newAddr, byte[] serial)
        {
            NewAddress = newAddr;
            Serial = serial;
        }

        public MsgIndividualAddressSerialWriteReq() { }



        public byte[] GetBytesCemi()
        {
            if(NewAddress == null)
                throw new Exception("NewAddress is required");
            TunnelRequest builder = new TunnelRequest();

            List<byte> data = new List<byte>();
            data.AddRange(Serial);

            data.AddRange(NewAddress.GetBytes());
            data.AddRange(new byte[] { 0, 0, 0, 0 });

            builder.Build(SourceAddress, MulticastAddress.FromString("0/0/0"), ApciTypes.IndividualAddressSerialNumberWrite, 255, data.ToArray());
            builder.SetPriority(Prios.System);

            data = new List<byte>() { 0x11, 0x00 };
            data.AddRange(builder.GetBytes());
            return data.ToArray();
        }

        public byte[] GetBytesEmi1()
        {
            throw new NotImplementedException("GetBytesEmi1 - MsgIndividualAddresSerialWriteReq");
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException("GetBytesEmi2 - MsgIndividualAddresSerialWriteReq");
        }


        public void ParseDataCemi()
        {
            if(Raw.Length < 8)
                throw new Exception("Invalid raw length");
                
            Serial = Raw.Take(6).ToArray();
            NewAddress = UnicastAddress.FromByteArray(Raw.Skip(6).Take(2).ToArray());
        }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgIndividualAddresSerialWriteReq");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgIndividualAddresSerialWriteReq");
        }
    }
}
