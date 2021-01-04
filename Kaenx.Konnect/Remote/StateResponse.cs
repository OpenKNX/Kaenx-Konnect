using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaenx.Konnect.Remote
{
    public class StateResponse : IRemoteMessage
    {
        public MessageCodes MessageCode { get; } = MessageCodes.State;
        public StateCodes Code { get; set; }
        public int SequenceNumber { get; set; } = -1;
        public int ChannelId { get; set; } = 0;
        public string Group { get; set; } = "";

        public string _key;
        public string _group;
        public string _code;

        public StateResponse() { }
        public StateResponse(StateCodes code, int sequence)
        {
            Code = code;
            SequenceNumber = sequence;
        }


        public void Parse(byte[] buffer)
        {
            SequenceNumber = buffer[1];
            Code = (StateCodes)buffer[2];
        }


        public ArraySegment<byte> GetBytes()
        {
            byte[] bytes = new byte[3];
            bytes[0] = Convert.ToByte(MessageCode);
            bytes[1] = Convert.ToByte(SequenceNumber);
            bytes[2] = Convert.ToByte(Code);

            return new ArraySegment<byte>(bytes);
        }
    }
}
