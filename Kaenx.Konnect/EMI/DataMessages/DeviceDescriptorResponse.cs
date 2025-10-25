using Kaenx.Konnect.Enums;
using Kaenx.Konnect.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.EMI.DataMessages
{
    public class DeviceDescriptorResponse : IDataMessage
    {
        public ApciTypes ApciType => StaticApciType;
        public static ApciTypes StaticApciType => ApciTypes.DeviceDescriptorResponse;

        public int DescriptorType { get; private set; }
        public byte[] DescriptorData { get; private set; } = Array.Empty<byte>();

        public DeviceDescriptorResponse(byte[] data, ExternalMessageInterfaces emi) {
            switch(emi)
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

        public DeviceDescriptorResponse(byte[] descriptorData, int descriptorType = 0)
        {
            DescriptorType = descriptorType;
            DescriptorData = descriptorData;
        }

        public byte[] GetBytesCemi()
        {
            List<byte> data = new List<byte>();
            data.Add((byte)(DescriptorType & 0x3F));
            data.AddRange(DescriptorData);
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
            DescriptorType = data[0] & 0x3F;
            DescriptorData = data.Skip(1).ToArray();
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
