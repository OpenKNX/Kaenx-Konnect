using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Messages;

namespace Kaenx.Konnect.Connections
{
    internal class UdpConnection : IDisposable
    {
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private UdpClient client;
        private IPEndPoint target;
        public delegate void ReceivedKnxMessage(IParserMessage message);
        public event ReceivedKnxMessage OnReceived;

        public UdpConnection(IPAddress ip, int port, IPEndPoint _target)
        {
            client = new UdpClient(new IPEndPoint(ip, port));
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Task.Run(ProcessReceive, tokenSource.Token);
            target = _target;
        }

        public async Task SendAsync(byte[] data)
        {
            await client.SendAsync(data, data.Length, target);
        }

        public IPEndPoint GetLocalEndpoint()
        {
            return client.Client.LocalEndPoint as IPEndPoint;
        }

        private async void ProcessReceive()
        {
            while(true)
            {
                var result = await client.ReceiveAsync();
                var knxResponse = ReceiverParserDispatcher.Instance.Build(result.Buffer);
                OnReceived?.Invoke(knxResponse);
            }
        }

        public void Dispose()
        {
            tokenSource.Cancel();
            client.Close();
            client.Dispose();
        }
    }
}