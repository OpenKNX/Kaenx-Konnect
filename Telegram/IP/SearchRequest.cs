using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.IP
{
    public class SearchRequest : IpTelegram
    {
        public SearchRequest(IPEndPoint endpoint, HostProtocols protocol = HostProtocols.IPv4_UDP) 
            : base(ServiceIdentifiers.SearchRequest)
        {
            Contents.Add(new Contents.HpaiContent(endpoint, protocol));
        }

        public SearchRequest(string ipAddress, int ipPort, HostProtocols protocol = HostProtocols.IPv4_UDP) 
            : base(ServiceIdentifiers.SearchRequest)
        {
            Contents.Add(new Contents.HpaiContent(new IPEndPoint(IPAddress.Parse(ipAddress), ipPort), protocol));
        }

        public override void Parse(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
