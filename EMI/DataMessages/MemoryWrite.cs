using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.EMI.DataMessages
{
    public class MemoryWrite : IDataMessage
    {
        public ApciTypes ApciType => StaticApciType;
        public static ApciTypes StaticApciType => ApciTypes.MemoryWrite;

        public uint Address { get; private set; }
        public uint Count { get; private set; }
        public byte[] Data { get; private set; } = Array.Empty<byte>();


        public MemoryWrite(uint address, uint count, byte[] data)
        {
            if(address > 0xFFFF)
                throw new ArgumentOutOfRangeException(nameof(address), "Address must be between 0 and 65535.");
            if (count > 0x3F)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be between 0 and 31.");
            Address = address;
            Count = count;
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public MemoryWrite(byte[] data, ExternalMessageInterfaces emi)
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
            data.Add((byte)Count);
            data.Add((byte)((Address >> 8) & 0xFF));
            data.Add((byte)(Address & 0xFF));
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
            Count = data[0];
            Address = ((uint)data[1] << 8) | data[2];
            Data = data.Skip(3).ToArray();
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
            return $"A={Address:X4} N={Count} ${BitConverter.ToString(Data).Replace("-", "")}";
        }
    }
}
