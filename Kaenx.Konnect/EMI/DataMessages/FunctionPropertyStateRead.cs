using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.EMI.DataMessages
{
    public class FunctionPropertyStateRead : IDataMessage
    {
        public ApciTypes ApciType => StaticApciType;
        public static ApciTypes StaticApciType => ApciTypes.FunctionPropertyStateRead;

        public int ObjectIndex { get; private set; }
        public int PropertyId { get; private set; }
        public byte[] Data { get; private set; } = Array.Empty<byte>();

        public FunctionPropertyStateRead(int objectIndex, int propertyId, byte[] data)
        {
            ObjectIndex = objectIndex;
            PropertyId = propertyId;
            Data = data;
        }

        public FunctionPropertyStateRead(byte[] data, ExternalMessageInterfaces emi)
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
            return $"OX={ObjectIndex} P={PropertyId} ${BitConverter.ToString(Data).Replace("-", "")}";
        }
    }
}
