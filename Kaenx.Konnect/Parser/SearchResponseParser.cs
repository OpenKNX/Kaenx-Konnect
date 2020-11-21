using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Responses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Parser
{
    class SearchResponseParser : IReceiveParser
    {
        public ushort ServiceTypeIdentifier => 0x0202;

        public IResponse Build(byte headerLength, byte protocolVersion, ushort totalLength, byte[] responseBytes)
        {
            SearchResponse resp = new SearchResponse();

            byte[] addr = new byte[4] { responseBytes[2], responseBytes[3], responseBytes[4], responseBytes[5] };

            resp.endpoint = new IPEndPoint(new IPAddress(addr), BitConverter.ToInt16(new byte[2] { responseBytes[7], responseBytes[6] },0));

            byte[] phAddr = new byte[2] { responseBytes[12], responseBytes[13] };
            resp.PhAddr = UnicastAddress.FromByteArray(phAddr);

            int total = Convert.ToInt32(responseBytes[8]);

            resp.FriendlyName = System.Text.Encoding.UTF8.GetString(responseBytes, 32, total - 32).Trim();
            return resp;
        }
    }
}
