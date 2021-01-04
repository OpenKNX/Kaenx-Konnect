using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Remote;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Messages
{
    public class MessageParser
    {

        public static IRemoteMessage Parse(byte[] data)
        {
            var headerLength = data[0];
            var protocolVersion = data[1];
            var serviceTypeIdentifier = BitConverter.ToUInt16(new[] { data[3], data[2] }, 0);
            var totalLength = BitConverter.ToUInt16(new[] { data[5], data[4] }, 0);

            return null;
        }
    }
}
