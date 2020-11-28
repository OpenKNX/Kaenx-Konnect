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
            resp.responseBytes = responseBytes;
            return resp;
        }
    }
}
