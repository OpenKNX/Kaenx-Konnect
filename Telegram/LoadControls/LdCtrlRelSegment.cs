using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kaenx.Konnect.Telegram.LoadControls
{
    public class LdCtrlRelSegment : ILoadControl
    {
        public LoadControlTypes LoadControlType => StaticLoadControlType;
        public static LoadControlTypes StaticLoadControlType => LoadControlTypes.LdCtrlRelSegment;

        public int LoadStateMachineIndex { get; private set; }
        public int Size { get; set; }
        private byte FillByte { get; set; }

        public Task Execute()
        {
            throw new NotImplementedException();
        }

        public void Parse(XElement xml)
        {
            LoadStateMachineIndex = int.Parse(xml.Attribute("LsmIdx")?.Value ?? throw new Exception("LsmIdx attribute missing in LdCtrlRelSegment element"));
            Size = int.Parse(xml.Attribute("Size")?.Value ?? throw new Exception("Size attribute missing in LdCtrlRelSegment element"));
            FillByte = byte.Parse(xml.Attribute("Fill")?.Value ?? throw new Exception("FillByte attribute missing in LdCtrlRelSegment element"));
        }
    }
}
