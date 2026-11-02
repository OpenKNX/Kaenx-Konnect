using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.IP.DIB
{
    public class ConnectionResponseData : IContent
    {
        public ConnectionTypes ConnectionType { get; private set; }
        public UnicastAddress LocalAddress { get; private set; }
        public int Length => 4;

        public ConnectionResponseData(ConnectionTypes connectionType, UnicastAddress localAddress)
        {
            ConnectionType = connectionType;
            LocalAddress = localAddress;
        }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Fügen Sie ggf. den „erforderlichen“ Modifizierer hinzu, oder deklarieren Sie den Modifizierer als NULL-Werte zulassend.
        public ConnectionResponseData(byte[] data)
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Fügen Sie ggf. den „erforderlichen“ Modifizierer hinzu, oder deklarieren Sie den Modifizierer als NULL-Werte zulassend.
        {
            Parse(data);
        }

        public void Parse(byte[] data)
        {
            if (data[0] != 4)
                throw new Exception("ConnectionResponseContent has not expected length: " + data[0]);

            ConnectionType = (ConnectionTypes)data[1];
            LocalAddress = UnicastAddress.FromByteArray(data.Skip(2).Take(2).ToArray());
        }

        public byte[] ToByteArray()
        {
            return new byte[] { (byte)Length, (byte)ConnectionType }.Concat(LocalAddress.GetBytes()).ToArray();
        }
    }
}
