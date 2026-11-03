using System.Net;
using System.Net.Sockets;
using Kaenx.Konnect;
using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.EMI.DataMessages;
using Kaenx.Konnect.EMI.LData;
using static UploadRegression;

static class ConfirmationRegression
{
    public static async Task Run()
    {
        using var gateway = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        var connection = KnxFactory.CreateTunnelingUdp((IPEndPoint)gateway.Client.LocalEndPoint!);
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(8));
        try
        {
            var connecting = connection.Connect();
            var connect = await gateway.ReceiveAsync(deadline.Token);
            var local = (IPEndPoint)gateway.Client.LocalEndPoint!;
            await gateway.SendAsync(new byte[] { 6, 0x10, 2, 6, 0, 20, 0x21, 0, 8, 1, 127, 0, 0, 1, (byte)(local.Port >> 8), (byte)local.Port, 4, 4, 0x11, 0xfe }, connect.RemoteEndPoint);
            await connecting;
            byte[]? previous = null;
            for (byte i = 0; i < 2; i++)
            {
                var message = new LDataBase(UnicastAddress.FromString("1.1.1"), false, 255,
                    new FunctionPropertyCommand(159, 40, new byte[] { (byte)(i + 1), 0, 1, 42 }));
                var sending = connection.SendAsync(message);
                var request = await gateway.ReceiveAsync(deadline.Token);
                await gateway.SendAsync(new byte[] { 6, 0x10, 4, 0x21, 0, 10, 4, 0x21, request.Buffer[8], 0 }, request.RemoteEndPoint);
                var wrong = previous?.ToArray() ?? request.Buffer.ToArray();
                wrong[8] = i; // Deliberately equals outbound counter, but confirms the wrong data.
                wrong[10] = 0x2e;
                if (previous == null) wrong[^1] ^= 1;
                await gateway.SendAsync(wrong, connection.GetLocalEndpoint());
                await gateway.ReceiveAsync(deadline.Token); // Must still ACK the unrelated confirmation.
                await Task.Delay(25, deadline.Token);
                Check(!sending.IsCompleted, "Unrelated confirmation completed current send");
                var correct = request.Buffer.ToList();
                correct[8] = (byte)(7 + i); // Server/client tunnel counters are independent.
                correct[10] = 0x2e;
                correct[14] = 0x11; correct[15] = 0xfe; // Gateway replaces source 0.0.0.
                correct[11] = 4;
                correct.InsertRange(12, new byte[] { 1, 2, 0, 0 }); // Optional cEMI metadata.
                correct[5] = (byte)correct.Count;
                previous = correct.ToArray();
                await gateway.SendAsync(previous, connection.GetLocalEndpoint());
                var ack = await gateway.ReceiveAsync(deadline.Token);
                Check(ack.Buffer[8] == 7 + i, "Confirmation ACK has wrong counter");
                await sending.WaitAsync(deadline.Token);
            }
            await connection.Disconnect();
        }
        finally { connection.Dispose(); }
    }
}