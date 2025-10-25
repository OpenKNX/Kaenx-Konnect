using Kaenx.Konnect.Addresses;
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
        public bool IsNumbered { get; } = false;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress? SourceAddress { get; set; }
        public IKnxAddress? DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.IndividualAddressWrite;
        public byte[] Raw { get; set; } = new byte[0];


        public UnicastAddress? NewAddress { get; set; }

        /// <summary>
        /// Creates a telegram to write an individual addres via programm button
        /// </summary>
        /// <param name="newAddress">New Unicast Address</param>
        public MsgIndividualAddressWriteReq(UnicastAddress newAddress)
        {
            NewAddress = newAddress;
        }

        public MsgIndividualAddressWriteReq() { }



        public byte[] GetBytesCemi()
        {
            if (NewAddress == null)
                throw new Exception("NewAddress is required");
            List<byte> data = new List<byte>() { 0x11, 0x00 };
            //TunnelRequest builder = new TunnelRequest();
            //builder.Build(SourceAddress, MulticastAddress.FromString("0/0/0"), ApciTypes.IndividualAddressWrite, 255, NewAddress.GetBytes());
            //builder.SetPriority(Prios.System);
            //data.AddRange(builder.GetBytes());
            return data.ToArray();
        }

        public byte[] GetBytesEmi1()
        {
            throw new NotImplementedException("GetBytesEmi1 - MsgIndividualAddressWriteReq");
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException("GetBytesEmi2 - MsgIndividualAddressWriteReq");
        }


        public void ParseDataCemi()
        {
            if (Raw.Length != 2)
                throw new Exception("Invalid raw Length");

            NewAddress = UnicastAddress.FromByteArray(Raw);
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
