using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Remote
{
    public interface IRemoteMessage
    {
        public int SequenceNumber { get; set; }
        public MessageCodes MessageCode { get; }
        public int ChannelId { get; set; }
        public void Parse(byte[] buffer);
        public ArraySegment<byte> GetBytes();
    }

    public enum MessageCodes
    {
        AuthRequest = 0x01,
        AuthResponse,
        CodesRequest,
        CodesResponse,
        ConnectRequest,
        ConnectResponse,
        SearchRequest,
        SearchResponse,

        State = 0xff
    }


    public enum StateCodes
    {
        Ok,
        WrongKey,
        GroupNotFound,
        WrongGroupKey,
        NotConnectedInterface
    }
}
