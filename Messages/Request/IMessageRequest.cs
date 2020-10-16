using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    public interface IMessageRequest
    {
        byte[] GetBytesEmi1();
        byte[] GetBytesEmi2();
        byte[] GetBytesCemi();
        void SetSequenzeNumb(int seq);
        void SetInfo(byte channel, byte seqCounter);
    }
}
