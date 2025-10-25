using System.Threading;
using Kaenx.Konnect.Messages.Response;

namespace Kaenx.Konnect.Classes.Helper
{
    public class AckHelper
    {
        public bool Ack { get; set; } = false;
        public CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();
    }
}