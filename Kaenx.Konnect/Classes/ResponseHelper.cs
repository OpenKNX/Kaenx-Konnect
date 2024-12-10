using System.Threading;
using Kaenx.Konnect.Messages.Response;

namespace Kaenx.Konnect.Classes
{
    public class ResponseHelper
    {
        public IMessageResponse? Response { get; set; } = null;
        public CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();
    }
}