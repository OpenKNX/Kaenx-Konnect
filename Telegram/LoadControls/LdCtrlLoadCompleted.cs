using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kaenx.Konnect.Telegram.LoadControls
{
    public class LdCtrlLoadCompleted : ILoadControl
    {
        public LoadControlTypes LoadControlType => StaticLoadControlType;
        public static LoadControlTypes StaticLoadControlType => LoadControlTypes.LdCtrlLoadCompleted;

        private int LoadStateMachineIndex { get; set; }

        public Task Execute()
        {
            throw new NotImplementedException();
        }

        public void Parse(XElement xml)
        {
            LoadStateMachineIndex = int.Parse(xml.Attribute("LsmIdx")?.Value ?? throw new Exception("LsmIdx attribute missing in LdCtrlLoadCompleted element"));
        }
    }
}
