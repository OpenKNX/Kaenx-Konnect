int failures = 0;
try { await ConfirmationRegression.Run(); Console.WriteLine("PASS: independent counters, wrong/stale confirmations, source replacement and metadata"); }
catch (Exception ex) { failures++; Console.WriteLine("FAIL: UDP confirmation: " + ex.Message); }
try { await TcpConfirmationRegression.Run(); Console.WriteLine("PASS: TCP confirmation with padded receive buffer"); }
catch (Exception ex) { failures++; Console.WriteLine("FAIL: TCP confirmation: " + ex.Message); }
return failures == 0 ? 0 : 1;

static class UploadRegression
{
    public static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}