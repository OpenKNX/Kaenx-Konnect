using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Responses;
using Kaenx.Konnect.Requests;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Parser
{
    class SearchRequestParser : IReceiveParser
    {
        public ushort ServiceTypeIdentifier => 0x0201;

        public IParserMessage Build(byte headerLength, byte protocolVersion, ushort totalLength, byte[] responseBytes)
        {
            SearchRequest resp = new SearchRequest();
            resp.responseBytes = responseBytes;
            return resp;
        }
    }
}
