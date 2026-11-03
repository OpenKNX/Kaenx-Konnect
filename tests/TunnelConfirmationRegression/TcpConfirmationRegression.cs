using System.Net;
using System.Net.Sockets;
using Kaenx.Konnect;
using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.EMI.DataMessages;
using Kaenx.Konnect.EMI.LData;

static class TcpConfirmationRegression
{
    public static async Task Run()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(8));
        try
        {
            var accept = listener.AcceptTcpClientAsync();
            var connection = KnxFactory.CreateTunnelingTcp((IPEndPoint)listener.LocalEndpoint);
            using var peer = await accept;
            var stream = peer.GetStream();
            try
            {
                var connecting = connection.Connect();
                await ReadFrame(stream, deadline.Token);
                await stream.WriteAsync(new byte[] { 6, 0x10, 2, 6, 0, 20, 0x21, 0, 8, 2, 127, 0, 0, 1, 0, 0, 4, 4, 0x11, 0xfe }, deadline.Token);
                await connecting;
                var message = new LDataBase(UnicastAddress.FromString("1.1.1"), false, 255, new GroupValueWrite(new byte[] { 1 }));
                var sending = connection.SendAsync(message);
                var request = await ReadFrame(stream, deadline.Token);
                request[10] = 0x2e;
                await stream.WriteAsync(request, deadline.Token);
                await sending.WaitAsync(deadline.Token);
                await connection.Disconnect();
            }
            finally { connection.Dispose(); }
        }
        finally { listener.Stop(); }
    }
    static async Task<byte[]> ReadFrame(NetworkStream stream, CancellationToken stop)
    {
        byte[] header = new byte[6];
        await stream.ReadExactlyAsync(header, stop);
        byte[] frame = new byte[(header[4] << 8) | header[5]];
        header.CopyTo(frame, 0);
        await stream.ReadExactlyAsync(frame.AsMemory(6), stop);
        return frame;
    }
}