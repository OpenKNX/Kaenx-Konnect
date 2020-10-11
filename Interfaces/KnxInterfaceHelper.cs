using Kaenx.Konnect.Connections;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Interfaces
{
    public class KnxInterfaceHelper
    {
        public static IKnxConnection GetConnection(IKnxInterface inter)
        {
            switch (inter)
            {
                case KnxInterfaceUsb interUsb:
                    return new KnxUsbTunneling(interUsb.DeviceId);

                case KnxInterfaceIp interIp:
                    return new KnxIpTunneling(new IPEndPoint(IPAddress.Parse(interIp.IP), interIp.Port));
            }

            throw new Exception("Für das Interface gibt es keine Connection");
        }
    }
}
