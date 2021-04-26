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
    /// Creates a telegram to write an individual addres via programm button
    /// </summary>
    public class MsgIndividualAddressWriteReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.IndividualAddressWrite;
        public byte[] Raw { get; set; }


        private UnicastAddress _address { get; set; }

        /// <summary>
        /// Creates a telegram to write an individual addres via programm button
        /// </summary>
        /// <param name="newAddress">New Unicast Address</param>
        public MsgIndividualAddressWriteReq(UnicastAddress newAddress)
        {
            _address = newAddress;
        }

        public MsgIndividualAddressWriteReq() { }



        public byte[] GetBytesCemi()
        {
            List<byte> data = new List<byte>() { 0x11, 0x00 };
            TunnelRequest builder = new TunnelRequest();
            builder.Build(MulticastAddress.FromString("0/0/0"), MulticastAddress.FromString("0/0/0"), ApciTypes.IndividualAddressWrite, 255, _address.GetBytes());
            builder.SetPriority(Prios.System);
            data.AddRange(builder.GetBytes());
            return data.ToArray();
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
            //TODO implement
        }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgIndividualAddressWriteReq");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgIndividualAddressWriteReq");
        }
    }
}
