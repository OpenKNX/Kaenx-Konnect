using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.IP
{
    public class TunnelFeatureGet : IpTelegram
    {
        public TunnelFeatureGet() 
            : base(ServiceIdentifiers.TunnelFeatureGet)
        {
        }

        public override void Parse(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
