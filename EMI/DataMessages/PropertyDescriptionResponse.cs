using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.EMI.DataMessages
{
    public class PropertyDescriptionResponse : IDataMessage
    {
        public ApciTypes ApciType => StaticApciType;
        public static ApciTypes StaticApciType => ApciTypes.PropertyDescriptionResponse;

        public uint ObjectIndex { get; private set; }
        public uint PropertyId { get; private set; }
        public uint PropertyIndex { get; private set; }
        public bool IsWriteEnabled { get; private set; }
        public uint MaxNumberOfElements { get; private set; }
        public uint ReadLevel { get; private set; }
        public uint WriteLevel { get; private set; }
        public PropertyDataTypes DataType { get; private set; }

        public PropertyDescriptionResponse(uint objectIndex, uint propertyId, PropertyDataTypes dataType, bool isWriteEnabled, uint maxNumberOfElements, uint readLevel, uint writeLevel, uint propertyIndex = 0)
        {
            if(objectIndex > 0xFF)
                throw new ArgumentOutOfRangeException(nameof(objectIndex), "Object Index must be between 0 and 255.");
            if(propertyId > 0xFF)
                throw new ArgumentOutOfRangeException(nameof(propertyId), "Property ID must be between 0 and 255.");
            if(propertyIndex > 0xFF)
                throw new ArgumentOutOfRangeException(nameof(propertyIndex), "Property Index must be between 0 and 255.");
            if(maxNumberOfElements > 0xFFF)
                throw new ArgumentOutOfRangeException(nameof(maxNumberOfElements), "Max Number Of Elements must be between 0 and 4095.");
            if(readLevel > 0xF)
                throw new ArgumentOutOfRangeException(nameof(readLevel), "Read Level must be between 0 and 15.");
            if(writeLevel > 0xF)
                throw new ArgumentOutOfRangeException(nameof(writeLevel), "Write Level must be between 0 and 15.");

            ObjectIndex = objectIndex;
            PropertyId = propertyId;
            PropertyIndex = propertyIndex;
            MaxNumberOfElements = maxNumberOfElements;
            ReadLevel = readLevel;
            WriteLevel = writeLevel;
            IsWriteEnabled = isWriteEnabled;
            DataType = dataType;
        }

        public PropertyDescriptionResponse(byte[] data, ExternalMessageInterfaces emi)
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
                    throw new NotImplementedException("Unknown ExternalMessageInterface: " + emi.ToString());
            }
        }

        public byte[] GetBytesCemi()
        {
            List<byte> data = new List<byte>();
            data.Add((byte)ObjectIndex);
            data.Add((byte)PropertyId);
            data.Add((byte)PropertyIndex);

            int octet11 = (int)DataType & 0x3F;
            if( IsWriteEnabled )
                octet11 |= 0x80;
            data.Add((byte)octet11);
            data.Add((byte)((MaxNumberOfElements >> 8) & 0x0F));
            data.Add((byte)(MaxNumberOfElements & 0xFF));

            int octet14 = (((int)ReadLevel & 0x0F) << 4);
            octet14 |= ((int)WriteLevel & 0x0F);
            data.Add((byte)octet14);

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
            PropertyIndex = data[2];

            IsWriteEnabled = (data[3] & 0x80) != 0;
            DataType = (PropertyDataTypes)(data[3] & 0x3F);

            MaxNumberOfElements = (uint)((data[4] & 0x0F) << 8 | data[5]);

            ReadLevel = (uint)((data[6] >> 4) & 0x0F);
            WriteLevel = (uint)(data[6] & 0x0F);
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
            return $"OX={ObjectIndex} PX={PropertyIndex} P={PropertyId} N={MaxNumberOfElements} A={ReadLevel}/{WriteLevel} W={IsWriteEnabled} T={DataType}";
        }
    }
}
