using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Kaenx.Konnect.EMI.DataMessages
{
    public class PropertyDescriptionRead : IDataMessage
    {
        public ApciTypes ApciType => StaticApciType;
        public static ApciTypes StaticApciType => ApciTypes.PropertyDescriptionRead;

        public uint ObjectIndex { get; private set; }
        public uint PropertyId { get; private set; }
        public uint PropertyIndex { get; private set; }

        public PropertyDescriptionRead(uint objectIndex, uint propertyId, uint propertyIndex = 0)
        {
            if(objectIndex > 0xFF)
                throw new ArgumentOutOfRangeException(nameof(objectIndex), "Object Index must be between 0 and 255.");
            if(propertyId > 0xFF)
                throw new ArgumentOutOfRangeException(nameof(propertyId), "Property ID must be between 0 and 255.");
            if(propertyIndex > 0xFF)
                throw new ArgumentOutOfRangeException(nameof(propertyIndex), "Property Index must be between 0 and 255.");

            ObjectIndex = objectIndex;
            PropertyId = propertyId;
            PropertyIndex = propertyIndex;
        }

        public PropertyDescriptionRead(byte[] data, ExternalMessageInterfaces emi)
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
            return $"OX={ObjectIndex} PX={PropertyIndex} P={PropertyId}";
        }
    }
}
