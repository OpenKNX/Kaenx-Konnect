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
    /// Creates a telegram to write an individual address via serialnumber
    /// </summary>
    public class MsgIndividualAddressSerialWriteReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.IndividualAddressSerialNumberWrite;
        public byte[] Raw { get; set; }


        private UnicastAddress _address { get; set; }
        private byte[] _serial { get; set; }

        /// <summary>
        /// Creates a telegram to write an individual address via serialnumber
        /// </summary>
        /// <param name="address">New Unicast Address</param>
        /// <param name="serial">Serialnumber</param>
        public MsgIndividualAddressSerialWriteReq(UnicastAddress newAddr, byte[] serial)
        {
            _address = newAddr;
            _serial = serial;
        }

        public MsgIndividualAddressSerialWriteReq() { }



        public byte[] GetBytesCemi()
        {
            TunnelRequest builder = new TunnelRequest();

            List<byte> data = new List<byte>();
            data.AddRange(_serial);

            data.AddRange(_address.GetBytes());
            data.AddRange(new byte[] { 0, 0, 0, 0 });

            builder.Build(SourceAddress, MulticastAddress.FromString("0/0/0"), Parser.ApciTypes.IndividualAddressSerialNumberWrite, 255, data.ToArray());
            builder.SetPriority(Prios.System);
            builder.SetChannelId(ChannelId);
            builder.SetSequence(SequenceCounter);
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


        public void ParseDataCemi()
        {
            throw new NotImplementedException("ParseDataCemi - MsgIndividualAddresSerialWriteReq");
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
