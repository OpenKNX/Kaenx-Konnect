using Kaenx.Konnect.Enums;

namespace Kaenx.Konnect.EMI.DataMessages
{
    public class DataSecureMessage : IDataMessage
    {
        public static ApciTypes StaticApciType => ApciTypes.DataSecure;
        public string GetDescription()
        {
            throw new System.NotImplementedException();
        }

        public ApciTypes ApciType => ApciTypes.DataSecure;

        // Roher Secure-APDU (alles nach den 2 APCI-Bytes):
        // [6 bytes SeqNum] [encrypted payload] [6 bytes MAC]
        public byte[] SecureApdu { get; }

        public DataSecureMessage(byte[] payload, ExternalMessageInterfaces emi)
        {
            SecureApdu = payload;
        }

        public byte[] GetBytesCemi() => SecureApdu;
        public byte[] GetBytesEmi1()
        {
            throw new System.NotImplementedException();
        }

        public byte[] GetBytesEmi2()
        {
            throw new System.NotImplementedException();
        }

        public void ParseDataCemi(byte[] data)
        {
            throw new System.NotImplementedException();
        }

        public void ParseDataEmi1(byte[] data)
        {
            throw new System.NotImplementedException();
        }

        public void ParseDataEmi2(byte[] data)
        {
            throw new System.NotImplementedException();
        }
    }
}