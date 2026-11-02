using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.EMI.DataMessages
{
    public class AdcResponse : IDataMessage
    {
        public ApciTypes ApciType => StaticApciType;
        public static ApciTypes StaticApciType => ApciTypes.ADCResponse;

        public uint Channel { get; private set; }
        public uint Count { get; private set; }
        public byte[] Data { get; private set; } = Array.Empty<byte>();

        public AdcResponse(uint channel, uint count, byte[] data)
        {
            if(channel > 0x7)
                throw new ArgumentOutOfRangeException(nameof(channel), "Channel must be between 0 and 7.");
            if(count > 0xFF)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be between 0 and 255.");
            if(data.Length != 2)
                throw new ArgumentException("Data length must be exactly 2 bytes.", nameof(data));

            Channel = channel;
            Count = count;
            Data = data;
        }

        public AdcResponse(byte[] data, ExternalMessageInterfaces emi)
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
            data.AddRange(Data);
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
            Data = data.Skip(2).ToArray();
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
            return $"#{Channel} N={Count} ${BitConverter.ToString(Data).Replace("-", "")}";
        }
    }
}
