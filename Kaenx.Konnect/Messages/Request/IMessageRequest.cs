using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    public interface IMessageRequest : IMessage
    {
        byte[] GetBytesEmi1();
        byte[] GetBytesEmi2();
        byte[] GetBytesCemi();


        //Todo implement it

        //void ParseDataCemi();
        //void ParseDataEmi1();
        //void ParseDataEmi2();
    }
}
