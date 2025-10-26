using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Connections;
using Kaenx.Konnect.Connections.Protocols;
using Kaenx.Konnect.Connections.Transports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect
{
    public class KnxFactory
    {
        public static IpKnxConnection CreateTunnelingUdp(string ip, int port)
        {
            return CreateTunnelingUdp(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        public static IpKnxConnection CreateTunnelingUdp(IPEndPoint endPoint)
        {
            ITransport connection = new UdpTransport(endPoint);
            TunnelingProtocol protocol = new TunnelingProtocol(connection);
            return new IpKnxConnection(protocol);
        }

        public static IpKnxConnection CreateRouting(UnicastAddress sourceAddress, string ip = "224.0.23.12", int port = 3671)
        {
            return CreateRouting(sourceAddress, new IPEndPoint(IPAddress.Parse(ip), port));
        }

        public static IpKnxConnection CreateRouting(UnicastAddress sourceAddress, IPEndPoint endPoint)
        {
            ITransport connection = new UdpTransport(IPAddress.Any, endPoint);
            RoutingProtocol protocol = new RoutingProtocol(sourceAddress, connection);
            return new IpKnxConnection(protocol);
        }
    }
}
