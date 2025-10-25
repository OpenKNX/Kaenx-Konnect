using Kaenx.Konnect.Enums;
using Kaenx.Konnect.Messages;
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

        public int ObjectIndex { get; private set; }
        public int PropertyId { get; private set; }
        public int PropertyIndex { get; private set; }
        public bool IsWriteEnabled { get; private set; }
        public int MaxNumberOfElements { get; private set; }
        public int ReadLevel { get; private set; }
        public int WriteLevel { get; private set; }
        public PropertyDataTypes DataType { get; private set; }

        public PropertyDescriptionResponse(int objectIndex, int propertyId, PropertyDataTypes dataType, bool isWriteEnabled, int maxNumberOfElements, int readLevel, int writeLevel, int propertyIndex = 0)
        {
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

            int octet14 = (ReadLevel & 0x0F) << 4;
            octet14 |= (WriteLevel & 0x0F);
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

            MaxNumberOfElements = ((data[4] & 0x0F) << 8) | data[5];

            ReadLevel = (data[6] >> 4) & 0x0F;
            WriteLevel = data[6] & 0x0F;
        }

        public void ParseDataEmi1(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void ParseDataEmi2(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
