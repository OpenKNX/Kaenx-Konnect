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
    internal class TcpTransport : ITransport
    {
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private TcpClient client;
        public NetworkInterface? Interface { get; set; }
        public int InterfaceIndex { get; set; } = 0;
        public bool IsAckRequired { get; set; } = false;

        public event ReceivedKnxMessage? OnReceived;
        private NetworkStream _stream;

        public TcpTransport(IPEndPoint _target)
        {
            IPAddress? ip = GetIpAddress(_target.Address.ToString());
            if(ip == null)
                throw new Exception("No suitable local IP address found for target " + _target.Address.ToString());
            Init(ip, _target);
        }

        public TcpTransport(IPAddress ip, IPEndPoint _target)
        {
            Init(ip, _target);
        }

        private void Init(IPAddress ip, IPEndPoint _target)
        {
            client = new TcpClient(new IPEndPoint(ip, 0));
            client.Connect(_target);
            _stream = client.GetStream();

            Task.Run(ProcessReceive, tokenSource.Token);
        }

        public async Task SendAsync(byte[] data)
        {
            ReadOnlyMemory<byte> mem = data;
            await _stream.WriteAsync(data, tokenSource.Token);
        }

        public IPEndPoint GetLocalEndpoint()
        {
            return new IPEndPoint(IPAddress.Any, 0);
        }

        public HostProtocols GetProtocolType()
        {
            if (client.Client.AddressFamily == AddressFamily.InterNetwork)
                return HostProtocols.IPv4_TCP;
            else if (client.Client.AddressFamily == AddressFamily.InterNetworkV6)
                return HostProtocols.IPv6_TCP;
            else
                throw new Exception("Unknown AddressFamily in TcpTransport: " + client.Client.AddressFamily.ToString());
        }

        private async void ProcessReceive()
        {
            Memory<byte> buffer = new byte[255];
            while (!tokenSource.IsCancellationRequested)
            {
                try
                {

                    int readed = await _stream.ReadAsync(buffer, tokenSource.Token);
                    if (OnReceived != null)
                        await OnReceived.Invoke(this, buffer.ToArray());
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