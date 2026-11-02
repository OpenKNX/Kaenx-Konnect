using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.IP.DIB
{
    public class SupportedServiceFamilies : IContent
    {
        public int Length { get; private set; } // depends on number of service families
        public DibTypes Type => DibTypes.SuppSvcFamilies;
        public List<(ServiceFamilies Family, byte Version)> ServiceFamilies { get; } = new List<(ServiceFamilies, byte)>();

        public SupportedServiceFamilies(List<(ServiceFamilies Family, byte Version)> supported)
        {
            supported.ForEach(s => ServiceFamilies.Add(s));
            Length = ServiceFamilies.Count * 2 + 2;
        }

        public SupportedServiceFamilies(byte[] data)
        {
            if (data[1] != (byte)Type)
                throw new ArgumentException("Data does not represent a SupportedDeviceFamilies DIB.");

            int length = data[0] - 2; // Subtracting the length byte and type byte
            int pos = 0;

            while(length > pos)
            {
                // TODO add ServiceFamily with version
                ServiceFamilies.Add(((ServiceFamilies)data[2 + pos], data[3 + pos]));
                pos += 2;
            }
            Length = ServiceFamilies.Count * 2 + 2;
        }

        public int GetServiceFamilyVersion(ServiceFamilies family)
        {
            var fam = ServiceFamilies.FirstOrDefault(f => f.Family == family);
            return fam == default ? 0 : fam.Version;
        }

        public byte[] ToByteArray()
        {
            List<byte> data = new List<byte>
            {
                (byte)Length,
                (byte)Type
            };

            foreach(var supported in ServiceFamilies)
            {
                data.Add((byte)supported.Family);
                data.Add(supported.Version);
            }

            return data.ToArray();
        }
    }
}
