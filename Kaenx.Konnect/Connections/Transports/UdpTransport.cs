using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Messages;
using static Kaenx.Konnect.Connections.Transports.ITransport;

namespace Kaenx.Konnect.Connections.Transports
{
    internal class UdpTransport : ITransport
    {
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private UdpClient client;
        private IPEndPoint? Source { get; set; }
        private IPEndPoint Target { get; set; }
        public NetworkInterface? Interface { get; set; }
        public int InterfaceIndex { get; set; } = 0;
        public bool IsAckRequired { get; set; } = true;

        public event ReceivedKnxMessage? OnReceived;

        public UdpTransport(IPAddress ip, IPEndPoint _target, bool isMulticast = false, IPEndPoint? _source = null)
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

        public UdpTransport(IPAddress ip, int port, IPEndPoint _target, bool isMulticast = false, IPEndPoint? _source = null)
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
            ReadOnlyMemory<byte> mem = data;
            await client.SendAsync(data, Target, tokenSource.Token);
        }

        public IPEndPoint GetLocalEndpoint()
        {
            return (IPEndPoint?)client.Client.LocalEndPoint ?? new IPEndPoint(IPAddress.Any, 0);
        }

        private async void ProcessReceive()
        {
            while (!tokenSource.IsCancellationRequested)
            {
                try
                {
                    var result = await client.ReceiveAsync(tokenSource.Token);
                    Debug.WriteLine($"Received {result.Buffer.Length} bytes");
                    if(OnReceived != null)
                        await OnReceived.Invoke(this, result.Buffer);
                }
                catch (OperationCanceledException)
                {
                    // Graceful exit
                    return;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error in UdpConnection ProcessReceive: " + ex.Message);
                    throw new Exception("Error in UdpConnection ProcessReceive", ex);
                }
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