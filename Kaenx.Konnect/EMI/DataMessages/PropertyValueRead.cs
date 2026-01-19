using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.EMI.DataMessages
{
    public class PropertyValueRead : IDataMessage
    {
        public ApciTypes ApciType => StaticApciType;
        public static ApciTypes StaticApciType => ApciTypes.PropertyValueRead;

        public int ObjectIndex { get; private set; }
        public int PropertyId { get; private set; }
        public int StartIndex { get; private set; }
        public int Count { get; private set; }


        public PropertyValueRead(int objectIndex, int propertyId, int startIndex, int count)
        {
            ObjectIndex = objectIndex;
            PropertyId = propertyId;
            StartIndex = startIndex;
            Count = count;
        }

        public PropertyValueRead(byte[] data, ExternalMessageInterfaces emi)
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

            int oct10 = (Count & 0x0F) << 4;
            oct10 |= (StartIndex >> 8) & 0x0F;
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
            StartIndex = ((data[2] & 0x0F) << 8) | data[3];
            Count = (data[2] >> 4) & 0x0F;
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
            return $"OX={ObjectIndex} P={PropertyId} I={StartIndex} N={Count}";
        }
    }
}
