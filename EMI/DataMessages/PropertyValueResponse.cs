using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.EMI.DataMessages
{
    public class PropertyValueResponse : IDataMessage
    {
        public ApciTypes ApciType => StaticApciType;
        public static ApciTypes StaticApciType => ApciTypes.PropertyValueResponse;

        public uint ObjectIndex { get; private set; }
        public uint PropertyId { get; private set; }
        public uint StartIndex { get; private set; }
        public uint Count { get; private set; }
        public byte[] Data { get; private set; } = Array.Empty<byte>();


        public PropertyValueResponse(uint objectIndex, uint propertyId, uint startIndex, uint count, byte[] data)
        {
            if(objectIndex > 0xFF)
                throw new ArgumentOutOfRangeException(nameof(objectIndex), "Object Index must be between 0 and 255.");
            if(propertyId > 0xFF)
                throw new ArgumentOutOfRangeException(nameof(propertyId), "Property ID must be between 0 and 255.");
            if(startIndex > 0xFFF)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "Start Index must be between 0 and 4095.");
            if(count > 0xF)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be between 0 and 15.");

            ObjectIndex = objectIndex;
            PropertyId = propertyId;
            StartIndex = startIndex;
            Count = count;
            Data = data;
        }

        public PropertyValueResponse(byte[] data, ExternalMessageInterfaces emi)
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
            data.Add((byte)(ObjectIndex & 0xFF));
            data.Add((byte)(PropertyId & 0xFF));

            int oct10 = ((int)Count & 0x0F) << 4;
            oct10 |= ((int)StartIndex >> 8) & 0x0F;
            data.Add((byte)(oct10));
            data.Add((byte)(StartIndex & 0xFF));
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
            ObjectIndex = data[0];
            PropertyId = data[1];
            StartIndex = (uint)(((data[2] & 0x0F) << 8) | data[3]);
            Count = (uint)((data[2] >> 4) & 0x0F);
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
            return $"OX={ObjectIndex} P={PropertyId} I={StartIndex} N={Count} ${BitConverter.ToString(Data).Replace("-", "")}";
        }
    }
}
