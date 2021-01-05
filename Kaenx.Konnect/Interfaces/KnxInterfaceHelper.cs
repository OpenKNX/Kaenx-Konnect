using Kaenx.Konnect.Connections;
using Kaenx.Konnect.Remote;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Interfaces
{
    public class KnxInterfaceHelper
    {
        public static IKnxConnection GetConnection(IKnxInterface inter, RemoteConnection conn)
        {
            switch (inter)
            {
                case KnxInterfaceUsb interUsb:
                    return new KnxUsbTunneling(interUsb.DeviceId);

                case KnxInterfaceIp interIp:
                    return new KnxIpTunneling(interIp.Endpoint); //TODO check for Routing

                case KnxInterfaceRemote interRem:
                    return new KnxRemote(interRem.RemoteHash, conn); // TODO check if needed
            }

            throw new Exception("Für das Interface gibt es keine Connection " + inter.ToString());
        }
    }
}
