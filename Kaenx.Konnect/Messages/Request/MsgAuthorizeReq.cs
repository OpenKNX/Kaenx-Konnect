using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using System;
using System.Collections.Generic;

namespace Kaenx.Konnect.Messages.Request
{
    /// <summary>
    /// Telegram to obtain access authorization
    /// </summary>
    public class MsgAuthorizeReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = true;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress? SourceAddress { get; set; }
        public IKnxAddress? DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.AuthorizeRequest;
        public byte[] Raw { get; set; } = new byte[0];


        public uint Key { get; set; }

        public MsgAuthorizeReq(uint key, UnicastAddress address)
        {
            Key = key;
            DestinationAddress = address;
        }

        public MsgAuthorizeReq() { }



        public byte[] GetBytesCemi()
        {
            TunnelRequest builder = new TunnelRequest();


            List<byte> data = new List<byte>() { };
            data.Add(0x00);
            data.Add((byte)(Key >> 24));
            data.Add((byte)(Key >> 16));
            data.Add((byte)(Key >> 8));
            data.Add((byte)Key);

            builder.Build(SourceAddress, DestinationAddress, ApciType, SequenceNumber, data.ToArray());

            data = new List<byte>() { 0x11, 0x00 };
            data.AddRange(builder.GetBytes());
            return data.ToArray();
        }

        public byte[] GetBytesEmi1()
        {
            Emi2Request builder = new Emi2Request();
            builder.Build(DestinationAddress, ApciTypes.AuthorizeRequest, SequenceNumber, BitConverter.GetBytes(Key));
            return builder.GetBytes();
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException();
        }


        public void ParseDataCemi()
        {
            if(Raw.Length < 5)
                throw new Exception("Invalid data length");
                
            Key = (uint)((Raw[1] << 24) | (Raw[2] << 16) | (Raw[3] << 8) | Raw[4]);
        }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgAuthorizeReq");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgAuthorizeReq");
        }
    }
}
