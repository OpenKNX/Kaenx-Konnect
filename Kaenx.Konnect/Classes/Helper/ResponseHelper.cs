using System.Threading;
using Kaenx.Konnect.EMI.DataMessages;
using Kaenx.Konnect.EMI.LData;
using Kaenx.Konnect.Messages.Response;

namespace Kaenx.Konnect.Classes.Helper
{
    public class ResponseHelper
    {
        public IDataMessage? Response { get; set; } = null;
        public CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();
    }
}