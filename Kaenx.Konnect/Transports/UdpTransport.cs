using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Enums;
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

        public UdpTransport(IPEndPoint _target, bool isMulticast = false, IPEndPoint? _source = null)
        {
            IPAddress? ip = GetIpAddress(_target.Address.ToString());
            if(ip == null)
                throw new Exception("No suitable local IP address found for target " + _target.Address.ToString());
            Init(ip, _target, isMulticast, _source);
        }

        public UdpTransport(IPAddress ip, IPEndPoint _target, bool isMulticast = false, IPEndPoint? _source = null)
        {
            Init(ip, _target, isMulticast, _source);
        }

        private void Init(IPAddress ip, IPEndPoint _target, bool isMulticast = false, IPEndPoint? _source = null)
        {
            client = new UdpClient(new IPEndPoint(ip, 0));
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
            if (isMulticast)
            {
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

        public HostProtocols GetProtocolType()
        {
            if (client.Client.AddressFamily == AddressFamily.InterNetwork)
                return HostProtocols.IPv4_UDP;
            else if (client.Client.AddressFamily == AddressFamily.InterNetworkV6)
                return HostProtocols.IPv6_UDP;
            else
                throw new Exception("Unknown AddressFamily in UdpTransport: " + client.Client.AddressFamily.ToString());
        }

        private async void ProcessReceive()
        {
            while (!tokenSource.IsCancellationRequested)
            {
                try
                {
                    var result = await client.ReceiveAsync(tokenSource.Token);
                    if (OnReceived != null)
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
        
        private IPAddress? GetIpAddress(string receiver)
        {
            if (receiver == "127.0.0.1")
                return IPAddress.Parse(receiver);

            IPAddress? IP = null;
            int mostipcount = 0;
            string[] ipParts = receiver.Split('.');

            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

            foreach(NetworkInterface adapter in adapters)
            {
                IPInterfaceProperties properties = adapter.GetIPProperties();
                foreach(UnicastIPAddressInformation addr in properties.UnicastAddresses)
                {
                    int sameCount = 0;
                    string[] hostParts = addr.Address.ToString().Split('.');
                    for (int i = 0; i < 4; i++)
                    {
                        if (ipParts[i] != hostParts[i])
                        {
                            if (sameCount > mostipcount)
                            {
                                IP = addr.Address;
                                mostipcount = sameCount;
                            }
                            break;
                        }
                        sameCount++;
                    }
                    if (sameCount > mostipcount)
                    {
                        IP = addr.Address;
                        mostipcount = sameCount;
                    }
                }
            }
            
            if (IP == null)
            {
                try
                {
                    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                    {
                        socket.Connect("8.8.8.8", 65530);
                        if(socket.LocalEndPoint != null)
                        {
                            IPEndPoint endPoint = (IPEndPoint)socket.LocalEndPoint;
                            IP = endPoint.Address;
                        }
                    }
                }
                catch { }
            }

            return IP;
        }

        public void Dispose()
        {
            tokenSource.Cancel();
            client.Close();
            client.Dispose();
        }
    }
}