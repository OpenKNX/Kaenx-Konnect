using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.IP
{
    public class KnxIpHeader
    {
        // This is fixed to 6
        public int HeaderLength { get; set; } = 6;
        // This is fixed to 1.0
        public int ProtocolVersion { get; set; } = 0x10;
        public ServiceIdentifiers ServiceIdentifier { get; set; } = ServiceIdentifiers.Unknown;
        public int TotalLength { get; set; } = 0;

        public KnxIpHeader(ServiceIdentifiers serviceIdentifier)
        {
            ServiceIdentifier = serviceIdentifier;
        }

        public KnxIpHeader(ServiceIdentifiers serviceIdentifier, int totalLength)
        {
            ServiceIdentifier = serviceIdentifier;
            TotalLength = totalLength;
        }

        public KnxIpHeader(byte[] data)
        {
            Parse(data);
        }

        public void Parse(byte[] data)
        {
            if (data.Length < HeaderLength)
                throw new ArgumentException("Data length is less than header length");
            HeaderLength = data[0];
            ProtocolVersion = data[1];
            ServiceIdentifier = (ServiceIdentifiers)BitConverter.ToUInt16(new byte[] { data[3], data[2] }, 0);
            TotalLength = BitConverter.ToUInt16(new byte[] { data[5], data[4] }, 0);
        }
    }
}
