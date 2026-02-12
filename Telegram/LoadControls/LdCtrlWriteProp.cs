using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kaenx.Konnect.Telegram.LoadControls
{
    public class LdCtrlWriteProp : ILoadControl
    {
        public LoadControlTypes LoadControlType => StaticLoadControlType;
        public static LoadControlTypes StaticLoadControlType => LoadControlTypes.LdCtrlWriteProp;

        public int ObjectIndex { get; private set; }
        public int PropertyId { get; private set; }
        private bool Verify { get; set; }
        public byte[] InlineData { get; set; } = Array.Empty<byte>();

        public Task Execute()
        {
            throw new NotImplementedException();
        }

        public void Parse(XElement xml)
        {
            ObjectIndex = int.Parse(xml.Attribute("ObjIdx")?.Value ?? throw new Exception("ObjIdx attribute missing in LdCtrlWriteProp element"));
            PropertyId = int.Parse(xml.Attribute("PropId")?.Value ?? throw new Exception("PropId attribute missing in LdCtrlWriteProp element"));
            Verify = bool.Parse(xml.Attribute("Verify")?.Value ?? throw new Exception("Verify attribute missing in LdCtrlWriteProp element"));
            InlineData = Convert.FromHexString(xml.Attribute("InlineData")?.Value ?? throw new Exception("InlineData attribute missing in LdCtrlWriteProp element"));
        }
    }
}
