using Device.Net;
using Kaenx.Konnect.Connections;
using Kaenx.Konnect.Remote;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Interfaces
{
    public class KnxInterfaceHelper
    {
        public delegate Task<IDevice> GetUsbDeviceHandler(KnxInterfaceUsb inter);

        public static async Task<IKnxConnection> GetConnection(IKnxInterface inter, RemoteConnection conn, GetUsbDeviceHandler getDevice)
        {
            switch (inter)
            {
                case KnxInterfaceUsb interUsb:
                    IDevice dev = await getDevice(interUsb);
                    return new KnxUsbTunneling(dev);

                case KnxInterfaceIp interIp:
                    return new KnxIpTunneling(interIp.Endpoint); //TODO check for Routing

                case KnxInterfaceRemote interRem:
                    KnxRemote rconn = new KnxRemote(interRem.RemoteHash, conn);
                    await rconn.Init(getDevice);
                    return rconn; // TODO check if needed
            }

            throw new Exception("Für das Interface gibt es keine Connection " + inter.ToString());
        }
    }
}
