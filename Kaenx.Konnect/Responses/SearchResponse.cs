using Kaenx.Konnect.Addresses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Responses
{
    public class SearchResponse :IResponse
    {
        public IPEndPoint endpoint;
        public string FriendlyName;
        public UnicastAddress PhAddr;
    }
}
