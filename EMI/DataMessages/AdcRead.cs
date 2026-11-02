using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.EMI.DataMessages
{
    public class AdcRead : IDataMessage
    {
        public ApciTypes ApciType => StaticApciType;
        public static ApciTypes StaticApciType => ApciTypes.ADCRead;

        public uint Channel { get; private set; }
        public uint Count { get; private set; }

        public AdcRead(uint channel, uint count)
        {
            if(channel > 0x7)
                throw new ArgumentOutOfRangeException(nameof(channel), "Channel must be between 0 and 7.");
            if(count > 0xFF)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be between 0 and 255.");

            Channel = channel;
            Count = count;
        }

        public AdcRead(byte[] data, ExternalMessageInterfaces emi)
        {
            switch (emi)
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
                    throw new NotSupportedException("The specified EMI type is not supported.");
            }
        }

        public byte[] GetBytesCemi()
        {
            List<byte> data = new List<byte>();
            data.Add((byte)(Channel & 0x3F));
            data.Add((byte)(Count & 0xFF));
            return data.ToArray();
        }

        public byte[] GetBytesEmi1()
        {
            throw new NotImplementedException();
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException();
        }

        public void ParseDataCemi(byte[] data)
        {
            Channel = (uint)(data[0] & 0x3F);
            Count = data[1];
        }

        public void ParseDataEmi1(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void ParseDataEmi2(byte[] data)
        {
            throw new NotImplementedException();
        }

        public string GetDescription()
        {
            return $"#{Channel} N={Count}";
        }
    }
}
