using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.IP.DIB
{
    public class ConnectionRequestInfo : IContent
    {
        public int Length => 4;
        public DibTypes Type => DibTypes.SuppSvcFamilies;
        public ConnectionTypes ConnectionType { get; private set; }
        public KnxLayers Layer { get; private set; }

        public ConnectionRequestInfo(byte[] data)
        {
            if(data.Length < Length)
                throw new ArgumentException("Data does not represent a ConnectionRequestInfo DIB.");

            ConnectionType = (ConnectionTypes)data[1];
            Layer = (KnxLayers)data[2];
        }

        public ConnectionRequestInfo(ConnectionTypes connectionType, KnxLayers layer)
        {
            ConnectionType = connectionType;
            Layer = layer;
        }

        public byte[] ToByteArray()
        {
            List<byte> data = new List<byte>
            {
                (byte)Length,
                (byte)ConnectionType,
                (byte)Layer,
                0x00 // Reserved
            };
            return data.ToArray();
        }
    }
}
