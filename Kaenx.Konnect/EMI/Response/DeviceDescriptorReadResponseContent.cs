using Kaenx.Konnect.EMI;
using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Messages.Response
{
    public class DeviceDescriptorReadResponseContent : IEmiConvertable
    {
        public int DescriptorType { get; private set; }
        public byte[] Data { get; private set; } = Array.Empty<byte>();

        public DeviceDescriptorReadResponseContent(byte[] data, int descriptorType = 0)
        {
            DescriptorType = descriptorType;
            Data = data;
        }

        public DeviceDescriptorReadResponseContent(byte[] data, ExternalMessageInterfaces emi)
        {
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

        public byte[] GetBytesCemi()
        {
            throw new NotImplementedException();
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
            Data = data.Skip(1).ToArray();
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
