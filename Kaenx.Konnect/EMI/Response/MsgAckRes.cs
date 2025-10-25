using Kaenx.Konnect.Addresses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Messages.Response
{
    public class MsgAckRes : IMessageResponse
    {
        public ApciTypes ApciType => ApciTypes.Ack;

        public byte[] GetBytesCemi()
        {
            throw new NotImplementedException();
        }
    }
}
