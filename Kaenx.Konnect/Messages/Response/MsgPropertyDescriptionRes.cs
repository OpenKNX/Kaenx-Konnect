using Kaenx.Konnect.Addresses;
using System;

namespace Kaenx.Konnect.Messages.Response
{
    public class MsgPropertyDescriptionRes : IMessageResponse
    {
        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = true;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.PropertyDescriptionResponse;
        public byte[] Raw { get; set; }



        public MsgPropertyDescriptionRes(byte[] data) => Raw = data;
        public MsgPropertyDescriptionRes() { }

        public byte ObjectIndex { get; set; }
        public byte PropertyId { get; set; }
        public byte PropertyIndex { get; set; }
        public bool Writable { get; set; }
        public byte Type { get; set; }
        public int MaxElements { get; set; }
        public byte ReadLevel { get; set; }
        public byte WriteLevel { get; set; }



        public void ParseDataCemi()
        {
            ObjectIndex = Raw[0];
            PropertyId = Raw[1];
            PropertyIndex = Raw[2];
            Writable = (Raw[3] & 0x80) != 0;
            Type = (byte)(Raw[3] & 0b00111111);
            MaxElements = (Raw[4] & 0x0f) << 8 | Raw[5];
            ReadLevel = (byte)(Raw[6] >> 4);
            WriteLevel = (byte)(Raw[6] & 0x0f);
        }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgPropertyDescriptionRes");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgPropertyDescriptionRes");
        }

        public byte[] GetBytesEmi1()
        {
            throw new NotImplementedException("GetBytesEmi1 - MsgPropertyDescriptionRes");
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException("getBytesEmi2 - MsgPropertyDescriptionRes");
        }

        public byte[] GetBytesCemi()
        {
            throw new NotImplementedException("GetBytesCemi - MsgPropertyDescriptionRes");
        }
    }
}
