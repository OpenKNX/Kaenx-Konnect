using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Parser;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Messages.Response
{
    public class MsgSearchRes : IMessageResponse
    {
        public byte ChannelId { get; set; }
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.PropertyValueWrite;
        public byte[] Raw { get; set; }



        public MsgSearchRes(byte[] data) => Raw = data;
        public MsgSearchRes() { }

        public IPEndPoint Endpoint { get; set; }
        public string FriendlyName { get; set; }
        public UnicastAddress PhAddr { get; set; }


        public void ParseDataCemi()
        {
            byte[] addr = new byte[4] { Raw[2], Raw[3], Raw[4], Raw[5] };
            Endpoint = new IPEndPoint(new IPAddress(addr), BitConverter.ToInt16(new byte[2] { Raw[7], Raw[6] }, 0));
            byte[] phAddr = new byte[2] { Raw[12], Raw[13] };
            PhAddr = UnicastAddress.FromByteArray(phAddr);
            int total = Convert.ToInt32(Raw[8]);
            FriendlyName = System.Text.Encoding.UTF8.GetString(Raw, 32, total - 32).Trim();
        }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgSearchRes");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgSearchRes");
        }
    }
}
