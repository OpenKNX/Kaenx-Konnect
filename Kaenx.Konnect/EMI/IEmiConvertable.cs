using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.EMI
{
    public interface IEmiConvertable
    {
        public byte[] GetBytesCemi();

        public byte[] GetBytesEmi1();

        public byte[] GetBytesEmi2();

        public void ParseDataCemi(byte[] data);

        public void ParseDataEmi1(byte[] data);

        public void ParseDataEmi2(byte[] data);
    }
}
