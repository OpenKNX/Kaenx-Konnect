using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using System;

namespace Kaenx.Konnect.Messages.Request
{
    /// <summary>
    /// Telegram to obtain access authorization
    /// </summary>
    public class MsgAuthorizeReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.AuthorizeRequest;
        public byte[] Raw { get; set; }


        public uint Key { get; set; }

        public MsgAuthorizeReq(uint key, UnicastAddress address)
        {
            Key = key;
            DestinationAddress = address;
        }

        public MsgAuthorizeReq() { }



        public byte[] GetBytesCemi()
        {
            TunnelCemiRequest builder = new TunnelCemiRequest();
            byte[] data = new byte[5];
            data[0] = 0;
            data[1] = (byte)(Key >> 24);
            data[2] = (byte)(Key >> 16);
            data[3] = (byte)(Key >> 8);
            data[4] = (byte)Key;

            builder.Build(UnicastAddress.FromString("0.0.0"), DestinationAddress, ApciType, SequenceNumber, data);
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
            Key = (uint)((Raw[1] << 24) | (Raw[2] << 16) | (Raw[3] << 8) | Raw[4]);
        }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgPropertyReadReq");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgPropertyReadReq");
        }
    }
}
