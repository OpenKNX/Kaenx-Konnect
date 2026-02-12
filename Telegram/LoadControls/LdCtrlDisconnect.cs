using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kaenx.Konnect.Telegram.LoadControls
{
    internal class LdCtrlDisconnect : ILoadControl
    {
        public LoadControlTypes LoadControlType => StaticLoadControlType;
        public static LoadControlTypes StaticLoadControlType => LoadControlTypes.LdCtrlDisconnect;

        public Task Execute()
        {
            throw new NotImplementedException();
        }

        public void Parse(XElement xml)
        {
            // nothing to do here
        }
    }
}
