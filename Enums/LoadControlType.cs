using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Enums
{
    public enum LoadControlTypes
    {
        LdCtrlConnect,
        LdCtrlUnload,
        LdCtrlDisconnect,
        LdCtrlLoad,
        LdCtrlRelSegment,
        LdCtrlWriteRelMem,
        LdCtrlWriteProp,
        LdCtrlLoadCompleted,
        LdCtrlRestart
    }
}
