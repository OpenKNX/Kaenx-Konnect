using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Connections
{
    public class KnxIpTunneling : IKnxConnection
    {

        public KnxIpTunneling(IPEndPoint sendEndPoint)
        {
            Port = GetFreePort();

            _sendEndPoint = sendEndPoint;
            _receiveEndPoint = new IPEndPoint(IPAddress.Any, Port);
            _udpClient = new UdpClient(_receiveEndPoint);
            _receiveParserDispatcher = new ReceiverParserDispatcher();
            _sendMessages = new BlockingCollection<byte[]>();

            ProcessSendMessages();
        }





        public static int GetFreePort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
}
