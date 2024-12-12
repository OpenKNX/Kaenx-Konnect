using System;
using System.Net;
using System.Net.NetworkInformation;
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
        private IPEndPoint? Source { get; set; }
        private IPEndPoint Target { get; set; }
        public delegate void ReceivedKnxMessage(UdpConnection sender, IParserMessage message);
        public event ReceivedKnxMessage? OnReceived;
        public NetworkInterface? Interface { get; set; }
        public int InterfaceIndex { get; set; } = 0;

        public UdpConnection(IPAddress ip, IPEndPoint _target, bool isMulticast = false, IPEndPoint? _source = null)
        {
            client = new UdpClient(new IPEndPoint(ip, 0));
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
            if(isMulticast) {
                client.Client.MulticastLoopback = false;
                client.MulticastLoopback = false;
                client.JoinMulticastGroup(_target.Address, ip);
            }
            Task.Run(ProcessReceive, tokenSource.Token);
            Target = _target;
            Source = _source;
        }

        public UdpConnection(IPAddress ip, int port, IPEndPoint _target, bool isMulticast = false, IPEndPoint? _source = null)
        {
            client = new UdpClient(new IPEndPoint(ip, port));
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
            if(isMulticast) {
                client.Client.MulticastLoopback = false;
                client.MulticastLoopback = false;
                client.JoinMulticastGroup(_target.Address, ip);
            }
            Task.Run(ProcessReceive, tokenSource.Token);
            Target = _target;
            Source = _source;
        }

        public async Task SendAsync(byte[] data)
        {
            await client.SendAsync(data, data.Length, Target);
        }

        public IPEndPoint GetLocalEndpoint()
        {
            return (IPEndPoint?)client.Client.LocalEndPoint ?? new IPEndPoint(IPAddress.Any, 0);
        }

        private async void ProcessReceive()
        {
            while(true)
            {
                var result = await client.ReceiveAsync();
                var knxResponse = ReceiverParserDispatcher.Instance.Build(result.Buffer);
                OnReceived?.Invoke(this, knxResponse);
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