using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Connections;
using Kaenx.Konnect.Connections.Protocols;
using Kaenx.Konnect.Connections.Transports;
using System.Net;
using System.Net.Sockets;

namespace Kaenx.Konnect
{
    public class KnxFactory
    {
        public static IKnxConnection CreateSerial(string portName)
        {
            var connection = new SerialTransport(portName);
            return new SerialKnxConnection(connection);
        }

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

        public static IpKnxConnection CreateTunnelingTcp(string ip, int port)
        {
            return CreateTunnelingTcp(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        public static IpKnxConnection CreateTunnelingTcp(IPEndPoint endPoint)
        {
            ITransport connection = new TcpTransport(endPoint);
            TunnelingProtocol protocol = new TunnelingProtocol(connection);
            return new IpKnxConnection(protocol);
        }

        public static IpKnxConnection CreateRouting(UnicastAddress sourceAddress, string ip = "224.0.23.12", int port = 3671)
        {
            return CreateRouting(sourceAddress, new IPEndPoint(IPAddress.Parse(ip), port));
        }

        public static IpKnxConnection CreateRouting(UnicastAddress sourceAddress, IPEndPoint endPoint)
        {
            var localIp = GetLocalIpForTarget(endPoint.Address);
            ITransport connection = new UdpTransport(localIp, endPoint, isMulticast: true);
            RoutingProtocol protocol = new RoutingProtocol(sourceAddress, connection);
            return new IpKnxConnection(protocol);
        }

        private static IPAddress GetLocalIpForTarget(IPAddress target)
        {
            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Connect(target, 3671);
                return ((IPEndPoint)socket.LocalEndPoint!).Address;
            }
            catch
            {
                return IPAddress.Any;
            }
        }
    }
}