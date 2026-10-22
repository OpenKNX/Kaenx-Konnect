using Kaenx.Konnect.Enums;
using System;

namespace Kaenx.Konnect.EMI.DataMessages
{
    public class NAK : IDataMessage
    {
        public ApciTypes ApciType => StaticApciType;
        public static ApciTypes StaticApciType => ApciTypes.NAK;

        public NAK() { }
        public NAK(byte[] data, ExternalMessageInterfaces emi) { }

        public byte[] GetBytesCemi() => Array.Empty<byte>();
        public byte[] GetBytesEmi1() => throw new NotImplementedException();
        public byte[] GetBytesEmi2() => throw new NotImplementedException();
        public void ParseDataCemi(byte[] data) { }
        public void ParseDataEmi1(byte[] data) => throw new NotImplementedException();
        public void ParseDataEmi2(byte[] data) => throw new NotImplementedException();
        public string GetDescription() => "NAK";
    }
}