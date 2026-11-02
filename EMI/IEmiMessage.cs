using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.EMI
{
    public interface IEmiMessage : IEmiConvertable
    {
        public MessageCodes MessageCode { get; }
    }
}
