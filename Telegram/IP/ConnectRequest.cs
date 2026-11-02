using Kaenx.Konnect.Enums;
using Kaenx.Konnect.Telegram.Contents;
using Kaenx.Konnect.Telegram.IP.DIB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.IP
{
    public class ConnectRequest : IpTelegram
    {
        public HpaiContent ControlEndpoint { get; private set; }
        public HpaiContent DataEndpoint { get; private set; }

        public ConnectRequest(IPEndPoint endpoint, HostProtocols protocol, ConnectionTypes connectionType = ConnectionTypes.Tunneling, KnxLayers layer = KnxLayers.LinkLayer) 
            : base(ServiceIdentifiers.ConnectRequest)
        {
            ControlEndpoint = new HpaiContent(endpoint, protocol);
            DataEndpoint = new HpaiContent(endpoint, protocol);
            Contents.Add(ControlEndpoint); // Control
            Contents.Add(DataEndpoint); // Data
            Contents.Add(new ConnectionRequestInfo(connectionType, layer));
        }

        public ConnectRequest(string ipAddress, int ipPort, HostProtocols protocol, ConnectionTypes connectionType = ConnectionTypes.Tunneling, KnxLayers layer = KnxLayers.LinkLayer) 
            : base(ServiceIdentifiers.ConnectRequest)
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), ipPort);
            ControlEndpoint = new HpaiContent(endpoint, protocol);
            DataEndpoint = new HpaiContent(endpoint, protocol);
            Contents.Add(ControlEndpoint); // Control
            Contents.Add(DataEndpoint); // Data
            Contents.Add(new ConnectionRequestInfo(connectionType, layer));
        }

        public ConnectRequest(byte[] data)
            : base(ServiceIdentifiers.ConnectRequest)
        {
            Parse(data);
        }

        public ConnectionRequestInfo? GetConnectionRequestInfo()
        {
            return Contents.OfType<ConnectionRequestInfo>().FirstOrDefault();
        }

        public override void Parse(byte[] data)
        {
            IEnumerable<byte> _data = data;
            Header.Parse(data);
            _data = _data.Skip(Header.HeaderLength);

            ControlEndpoint = new HpaiContent(_data.ToArray());
            Contents.Add(ControlEndpoint);
            _data = _data.Skip(ControlEndpoint.Length);

            DataEndpoint = new HpaiContent(_data.ToArray());
            Contents.Add(DataEndpoint);
            _data = _data.Skip(DataEndpoint.Length);

            ConnectionRequestInfo connectionRequestInfo = new ConnectionRequestInfo(_data.ToArray());
            Contents.Add(connectionRequestInfo);
        }
    }
}
