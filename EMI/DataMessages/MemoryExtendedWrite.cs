using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.EMI.DataMessages
{
    public class MemoryExtendedWrite : IDataMessage
    {
        public ApciTypes ApciType => StaticApciType;
        public static ApciTypes StaticApciType => ApciTypes.MemoryExtendedWrite;

        public uint Address { get; private set; }
        public uint Count { get; private set; }
        public byte[] Data { get; private set; } = Array.Empty<byte>();


        public MemoryExtendedWrite(uint address, uint count, byte[] data)
        {
            if(address > 0xFFFFFF)
                throw new ArgumentOutOfRangeException(nameof(address), "Address must be between 0 and 16777215.");
            if (count > 0xFF)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be between 0 and 255.");

            Address = address;
            Count = count;
            Data = data;
        }

        public MemoryExtendedWrite(byte[] data, ExternalMessageInterfaces emi)
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
            data.Add((byte)(Address & 0xFF));
            data.Add((byte)((Address >> 8) & 0xFF));
            data.Add((byte)((Address >> 16) & 0xFF));
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
            Address = (uint)((data[1] << 16) | (data[2] << 8) | data[3]);
            Data = data.Skip(4).ToArray();
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
            return $"Address={Address:X6} Count={Count} {BitConverter.ToString(Data).Replace("-", "")}";
        }
    }
}
