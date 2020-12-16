using Kaenx.Konnect.Classes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Builders
{
    class SearchRequest : IRequestBuilder
    {
        private List<byte> bytes = new List<byte>();


        public void Build(IPEndPoint source)
        {
            byte[] header = { 0x06, 0x10, 0x02, 0x01, 0x00, 0x0e }; // Length, Version, Descriptor 2x, Total length 2x
            bytes.AddRange(header);

            bytes.AddRange(new HostProtocolAddressInformation(0x01, source).GetBytes());
        }

        public byte[] GetBytes()
        {
            return bytes.ToArray();
        }

        public void SetChannelId(byte channelId) { }

        public void SetSequence(byte sequence) { }
    }
}
