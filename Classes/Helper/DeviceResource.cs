using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Classes.Helper
{
    public class DeviceResource
    {
        public AddressSpace AddressSpace { get; set; }

        public uint InterfaceObjectRef { get; set; }
        public uint PropertyID { get; set; }
        public uint Address { get; set; }

        public int Length { get; set; }
    }
}
