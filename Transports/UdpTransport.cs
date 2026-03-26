using Kaenx.Konnect.Connections.Transports;
using Kaenx.Konnect.Enums;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using static Kaenx.Konnect.Connections.Transports.ITransport;

internal class UdpTransport : ITransport
{
    private CancellationTokenSource _cts = new();
    private UdpClient _client;
    private IPEndPoint _target;
    private IPEndPoint? _source;
    private bool _isMulticast;
    private IPAddress _localIp;

    private readonly Channel<byte[]> _receiveChannel =
        Channel.CreateBounded<byte[]>(new BoundedChannelOptions(512)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true
        });

    public event ReceivedKnxMessage? OnReceived;

    public UdpTransport(IPAddress ip, IPEndPoint target, bool isMulticast = false, IPEndPoint? source = null)
    {
        if (ip == null)
            throw new Exception("No suitable local IP address found for " + target.Address);
        _localIp = ip;
        _target = target;
        _source = source;
        _isMulticast = isMulticast;
        BuildSocket();
        StartLoops();
    }

    public UdpTransport(IPEndPoint target, bool isMulticast = false, IPEndPoint? source = null)
    {
        IPAddress? ip = GetIpAddress(target.Address.ToString());
        if (ip == null)
            throw new Exception("No suitable local IP address found for " + target.Address);
        _localIp = ip;
        _target = target;
        _source = source;
        _isMulticast = isMulticast;
        BuildSocket();
        StartLoops();
    }

    private void BuildSocket()
    {
        if (_isMulticast)
            _client = new UdpClient(new IPEndPoint(IPAddress.Any, _target.Port));
        else
            _client = new UdpClient(new IPEndPoint(_localIp, 0));

        _client.Client.ReceiveBufferSize = 256 * 1024;
        _client.Client.SendBufferSize = 64 * 1024;

        _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);

        if (_isMulticast)
        {
            _client.Client.MulticastLoopback = false;
            _client.MulticastLoopback = false;
            _client.JoinMulticastGroup(_target.Address, _localIp);
        }
    }

    private void StartLoops()
    {
        _ = Task.Run(ReceiveLoopAsync, _cts.Token);
        _ = Task.Run(ProcessLoopAsync, _cts.Token);
    }

    private async Task ReceiveLoopAsync()
    {
        int reconnectDelay = 500;

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                var result = await _client.ReceiveAsync(_cts.Token);
                _receiveChannel.Writer.TryWrite(result.Buffer);
                reconnectDelay = 500;
            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex) when (ex.InnerException is OperationCanceledException) { return; }
            catch (ObjectDisposedException) { return; }
            catch (SocketException sex)
            {
                Debug.WriteLine($"[UdpTransport] SocketException: {sex.SocketErrorCode} – versuche Reconnect...");
                await TryReconnectAsync(reconnectDelay);
                reconnectDelay = Math.Min(reconnectDelay * 2, 10_000);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UdpTransport] Unerwarteter Fehler im ReceiveLoop: {ex}");
                await TryReconnectAsync(reconnectDelay);
            }
        }
    }

    private async Task ProcessLoopAsync()
    {
        await foreach (var data in _receiveChannel.Reader.ReadAllAsync(_cts.Token))
        {
            if (OnReceived == null) continue;
            try
            {
                await OnReceived.Invoke(this, data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UdpTransport] Fehler in OnReceived-Handler: {ex}");
            }
        }
    }

    private async Task TryReconnectAsync(int delayMs)
    {
        try
        {
            _client.Close();
            _client.Dispose();
        }
        catch { }

        await Task.Delay(delayMs, _cts.Token).ConfigureAwait(false);

        try
        {
            BuildSocket();
            Debug.WriteLine("[UdpTransport] Socket neu aufgebaut.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UdpTransport] Reconnect fehlgeschlagen: {ex.Message}");
        }
    }

    public bool IsAckRequired { get; set; } = true;

    public async Task SendAsync(byte[] data)
    {
        try
        {
            await _client.SendAsync(data, _target, _cts.Token);
        }
        catch (SocketException sex)
        {
            Debug.WriteLine($"[UdpTransport] SendAsync SocketException: {sex.SocketErrorCode}");
            throw;
        }
    }

    private IPAddress? GetIpAddress(string receiver)
    {
        if (receiver == "127.0.0.1")
            return IPAddress.Parse(receiver);

        IPAddress? best = null;
        int bestScore = 0;
        string[] targetParts = receiver.Split('.');

        foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (adapter.OperationalStatus != OperationalStatus.Up) continue;

            foreach (var addr in adapter.GetIPProperties().UnicastAddresses)
            {
                if (addr.Address.AddressFamily != AddressFamily.InterNetwork) continue;

                string[] hostParts = addr.Address.ToString().Split('.');
                int score = 0;
                for (int i = 0; i < 4; i++)
                {
                    if (targetParts[i] != hostParts[i]) break;
                    score++;
                }
                if (score > bestScore)
                {
                    best = addr.Address;
                    bestScore = score;
                }
            }
        }

        if (best == null)
        {
            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                socket.Connect("8.8.8.8", 65530);
                if (socket.LocalEndPoint is IPEndPoint ep)
                    best = ep.Address;
            }
            catch { }
        }

        return best;
    }

    public IPEndPoint GetLocalEndpoint() =>
        (IPEndPoint?)_client.Client.LocalEndPoint ?? new IPEndPoint(IPAddress.Any, 0);

    public HostProtocols GetProtocolType() =>
        _client.Client.AddressFamily switch
        {
            AddressFamily.InterNetwork => HostProtocols.IPv4_UDP,
            AddressFamily.InterNetworkV6 => HostProtocols.IPv6_UDP,
            _ => throw new Exception("Unknown AddressFamily: " + _client.Client.AddressFamily)
        };

    public void Dispose()
    {
        _cts.Cancel();
        _receiveChannel.Writer.TryComplete();
        _client.Close();
        _client.Dispose();
        _cts.Dispose();
    }
}
