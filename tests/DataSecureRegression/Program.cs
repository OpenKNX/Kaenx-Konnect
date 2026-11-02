using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Kaenx.Konnect;
using Kaenx.Konnect.EMI.DataMessages;
using Kaenx.Konnect.Telegram.IP;

// Fails if Secure packets kill the receiver, suppress ACKs, leak to the application,
// or prevent ordinary telegrams from being received after a Secure packet.
using var listener = new WarningListener();
Trace.Listeners.Add(listener);
try
{
    if (args.Contains("--udp"))
        await UdpRegression();
    else
        ParserRegression();
    Console.WriteLine("PASS: " + (args.Contains("--udp") ? "UDP reception, ACKs and 1,000 interleaved telegrams" : "Secure filtering, warning and unrelated errors"));
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine("FAIL: " + ex);
    return 1;
}
finally
{
    Trace.Listeners.Remove(listener);
}

void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

void ParserRegression()
{
    foreach (bool additionalInfo in new[] { false, true })
    foreach (bool numbered in new[] { false, true })
    foreach (byte code in new byte[] { 0x29, 0x2e })
    {
        int before = listener.Messages.Count;
        var request = new TunnelingRequest(Packet(0x31, code, true, additionalInfo, numbered));
        Check(request.GetConnectionHeader().ChannelId == 0x15, "Channel lost");
        Check(request.GetConnectionHeader().SequenceCounter == 0x31, "Sequence lost");
        Check((byte)request.MessageCode == code, "Message code lost");
        Check(request.Contents.Count == 1, "Unsupported encrypted content must not reach consumers");
        Check(listener.Messages.Count > before && listener.Messages.Last().Contains("DataSecure"), "No Secure warning emitted");
    }
    byte[] unknown = Packet(0, 0x29, true);
    unknown[20] = 0xfe; // APCI 0x3fe has no implementation; must still throw.
    try { _ = new TunnelingRequest(unknown); throw new Exception("Unrelated unsupported APCI was swallowed"); }
    catch (NotImplementedException) { }
    try { _ = new TunnelingRequest(new byte[3]); throw new Exception("Malformed packet was swallowed"); }
    catch (ArgumentException) { }
}

async Task UdpRegression()
{
    using var gateway = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
    var connection = KnxFactory.CreateTunnelingUdp((IPEndPoint)gateway.Client.LocalEndPoint!);
    try
    {
        int received = 0;
        int unexpected = 0;
        connection.OnReceivedMessage += message =>
        {
            if (message.Content is GroupValueWrite write && write.Data.SequenceEqual(new byte[] { 1 }))
                Interlocked.Increment(ref received);
            else
                Interlocked.Increment(ref unexpected);
        };
        var target = connection.GetLocalEndpoint();
        for (int i = 0; i < 1000; i++)
        {
            bool secure = i % 2 == 0;
            byte sequence = (byte)i;
            byte code = secure && i % 4 == 0 ? (byte)0x2e : (byte)0x29;
            await gateway.SendAsync(Packet(sequence, code, secure, i % 3 == 0, false), target);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var ack = await gateway.ReceiveAsync(timeout.Token);
            Check(ack.Buffer.SequenceEqual(new byte[] { 6, 0x10, 4, 0x21, 0, 10, 4, 0x15, sequence, 0 }), $"Incorrect ACK #{i}");
        }
        // The receive callback runs immediately after the ACK; allow it to finish.
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        while (Volatile.Read(ref received) < 500)
            await Task.Delay(1, deadline.Token);
        Check(received == 500 && unexpected == 0, "Secure filtering or ordinary payload delivery failed");
        Trace.Flush();
        Check(listener.Messages.Count == 500, "Expected one warning for each ignored Secure telegram");
    }
    finally
    {
        connection.Dispose();
    }
}

static byte[] Packet(byte sequence, byte code, bool secure, bool additionalInfo = false, bool numbered = false)
{
    // KNXnet/IP 0x0420, channel 0x15; source 1.1.1, group 0/0/1;
    // APCI DataSecure = 0x3f1, ordinary GroupValueWrite(value=1) = 0x081.
    var bytes = new List<byte> { 6, 0x10, 4, 0x20, 0, 0, 4, 0x15, sequence, 0, code, (byte)(additionalInfo ? 4 : 0) };
    if (additionalInfo) bytes.AddRange(new byte[] { 1, 2, 0, 0 });
    bytes.AddRange(new byte[] { 0xb0, 0xe0, 0x11, 1, 0, 1, (byte)(secure ? 16 : 1), (byte)((numbered ? 0x40 : 0) | (secure ? 3 : 0)), (byte)(secure ? 0xf1 : 0x81) });
    if (secure) bytes.AddRange(new byte[15]); // Opaque synthetic secure envelope, never decrypted.
    bytes[5] = (byte)bytes.Count;
    return bytes.ToArray();
}


sealed class WarningListener : TraceListener
{
    public System.Collections.Concurrent.ConcurrentQueue<string> Messages { get; } = new();
    public override void Write(string? message) { }
    public override void WriteLine(string? message) { }
    public override void TraceEvent(TraceEventCache? cache, string source, TraceEventType eventType, int id, string? message)
    {
        if (eventType == TraceEventType.Warning) Messages.Enqueue(message ?? "");
    }
}