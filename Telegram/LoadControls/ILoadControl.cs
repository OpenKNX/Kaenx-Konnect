using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kaenx.Konnect.Telegram.LoadControls
{
    public interface ILoadControl
    {
        public LoadControlTypes LoadControlType { get; }
        public static LoadControlTypes StaticLoadControlType { get; }

        public Task Execute();
        public void Parse(XElement xml);
    }
}
