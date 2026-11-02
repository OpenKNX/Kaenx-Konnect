using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.EMI.DataMessages
{
    public interface IDataMessage : IEmiConvertable
    {
        public ApciTypes ApciType { get; }
        public static ApciTypes StaticApciType { get; }
        public string GetDescription();
    }
}
