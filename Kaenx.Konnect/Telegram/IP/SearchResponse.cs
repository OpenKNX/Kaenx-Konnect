using Kaenx.Konnect.Enums;
using Kaenx.Konnect.Telegram.Contents;
using Kaenx.Konnect.Telegram.IP.DIB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.IP
{
    public class SearchResponse : IpTelegram
    {
        public SearchResponse(IPEndPoint endpoint, HostProtocols protocol)
            : base(ServiceIdentifiers.SearchResponse)
        {
            Contents.Add(new HpaiContent(endpoint, protocol));
        }

        public SearchResponse(string ipAddress, int ipPort, HostProtocols protocol)
            : base(ServiceIdentifiers.SearchResponse)
        {
            Contents.Add(new HpaiContent(new IPEndPoint(IPAddress.Parse(ipAddress), ipPort), protocol));
        }

        public SearchResponse(byte[] data)
            : base(ServiceIdentifiers.SearchResponse)
        {
            Parse(data);
        }

        public HpaiContent? GetEndpoint()
        {
            return Contents.OfType<HpaiContent>().FirstOrDefault();
        }

        public DeviceInfo? GetDeviceInfo()
        {
            return Contents.OfType<DeviceInfo>().FirstOrDefault();
        }

        public override void Parse(byte[] data)
        {
            IEnumerable<byte> _data = data;
            Header.Parse(data);
            _data = _data.Skip(Header.HeaderLength);

            HpaiContent hpai = new HpaiContent(_data.ToArray());
            Contents.Add(hpai);
            _data = _data.Skip(hpai.Length);

            while(_data.Any())
            {
                DibTypes dibType = (DibTypes)_data.ElementAt(1);

                switch(dibType)
                {
                    case DibTypes.DeviceInfo:
                        DeviceInfo deviceInfo = new DeviceInfo(_data.ToArray());
                        Contents.Add(deviceInfo);
                        _data = _data.Skip(deviceInfo.Length);
                        break;
                    case DibTypes.SuppSvcFamilies:
                        SupportedServiceFamilies suppSvcFamilies = new SupportedServiceFamilies(_data.ToArray());
                        Contents.Add(suppSvcFamilies);
                        _data = _data.Skip(suppSvcFamilies.Length);
                        break;
                    default:
                        // Unknown DIB type, stop parsing
                        Debug.WriteLine($"Unknown DIB type: {_data.ElementAt(1)}, stopping parsing.");
                        _data = _data.Skip(_data.First());
                        break;
                }
            }
        }
    }
}
