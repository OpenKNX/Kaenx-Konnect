using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.EMI.DataMessages
{
    public class Restart : IDataMessage
    {
        public ApciTypes ApciType => StaticApciType;
        public static ApciTypes StaticApciType => ApciTypes.Restart;

        public Restart()
        {
            // Nothing to do for Connect Request
        }

        public Restart(byte[] data, ExternalMessageInterfaces emi)
        {
            switch(emi)
            {
                case ExternalMessageInterfaces.cEmi:
                    ParseDataCemi(data);
                    break;
                case ExternalMessageInterfaces.Emi1:
                    ParseDataEmi1(data);
                    break;
                case ExternalMessageInterfaces.Emi2:
                    ParseDataEmi2(data);
                    break;
                default:
                    throw new NotImplementedException("Unknown ExternalMessageInterface: " + emi.ToString());
            }
        }

        public byte[] GetBytesCemi()
        {
            // Nothing to do for Connect Request
            return Array.Empty<byte>();
        }

        public byte[] GetBytesEmi1()
        {
            // Nothing to do for Connect Request
            return Array.Empty<byte>();
        }

        public byte[] GetBytesEmi2()
        {
            // Nothing to do for Connect Request
            return Array.Empty<byte>();
        }

        public void ParseDataCemi(byte[] data)
        {
            // Nothing to do for Connect Request
        }

        public void ParseDataEmi1(byte[] data)
        {
            // Nothing to do for Connect Request
        }

        public void ParseDataEmi2(byte[] data)
        {
            // Nothing to do for Connect Request
        }

        public string GetDescription()
        {
            return "";
        }
    }
}
