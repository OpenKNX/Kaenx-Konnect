using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kaenx.Konnect.Telegram.LoadControls
{
    public class LdCtrlWriteRelMem : ILoadControl
    {
        public LoadControlTypes LoadControlType => StaticLoadControlType;
        public static LoadControlTypes StaticLoadControlType => LoadControlTypes.LdCtrlWriteRelMem;

        public int ObjectIndex { get; private set; }
        private int Offset { get; set; }
        private int Size { get; set; }
        private bool Verify { get; set; }
        public List<byte> Data { get; set; } = new List<byte>();

        public Task Execute()
        {
            throw new NotImplementedException();
        }

        public void Parse(XElement xml)
        {
            ObjectIndex = int.Parse(xml.Attribute("ObjIdx")?.Value ?? throw new Exception("ObjIdx attribute missing in LdCtrlWriteRelMem element"));
            Offset = int.Parse(xml.Attribute("Offset")?.Value ?? throw new Exception("Offset attribute missing in LdCtrlWriteRelMem element"));
            Size = int.Parse(xml.Attribute("Size")?.Value ?? throw new Exception("Size attribute missing in LdCtrlWriteRelMem element"));
            Verify = bool.Parse(xml.Attribute("Verify")?.Value ?? throw new Exception("Verify attribute missing in LdCtrlWriteRelMem element"));
        }
    }
}
