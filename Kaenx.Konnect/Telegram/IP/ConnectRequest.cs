using Kaenx.Konnect.Enums;
using Kaenx.Konnect.Telegram.IP.DIB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.IP
{
    public class ConnectRequest : IpTelegram
    {
        public ConnectRequest(IPEndPoint endpoint, HostProtocols protocol, ConnectionTypes connectionType = ConnectionTypes.Tunneling, KnxLayers layer = KnxLayers.LinkLayer) 
            : base(ServiceIdentifiers.ConnectRequest)
        {
            Initialize(endpoint, protocol, connectionType, layer);
        }

        public ConnectRequest(string ipAddress, int ipPort, HostProtocols protocol, ConnectionTypes connectionType = ConnectionTypes.Tunneling, KnxLayers layer = KnxLayers.LinkLayer) 
            : base(ServiceIdentifiers.ConnectRequest)
        {
            Initialize(new IPEndPoint(IPAddress.Parse(ipAddress), ipPort), protocol, connectionType, layer);
        }

        private void Initialize(IPEndPoint endPoint, HostProtocols protocol, ConnectionTypes connectionType = ConnectionTypes.Tunneling, KnxLayers layer = KnxLayers.LinkLayer)
        {
            var hpai = new Contents.HpaiContent(endPoint, protocol);
            Contents.Add(hpai); // Control
            Contents.Add(hpai); // Data
            Contents.Add(new ConnectionRequestInfo(connectionType, layer));
        }

        public override void Parse(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
