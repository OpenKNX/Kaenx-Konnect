using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Kaenx.Konnect.Enums
{
    public enum IpErrors
    {
        NoError = 0x00,
        // The Requested Protocol is not supported by the KNXnet/IP Server device.
        HostProtocolType = 0x01,
        // The KNXnet/IP Server device does not support the requested KNXnet/IP protocol version.
        VersionNotSupported = 0x02,
        // The KNXnet/IP Server device cannot process the request because the sequence number is not correct.
        SequenceNumber = 0x03,
        // The KNXnet/IP Server device cannot find an active data connection with the specified ID. 
        ConnectionId = 0x21,
        // The requested connection type is not supported by the KNXnet/IP Server device.
        ConnectionType = 0x22,
        // One or more requested connection options are not supported by the KNXnet/IP Server device. 
        ConnectionOption = 0x23,
        // The KNXnet/IP Server device cannot accept the new data connection because its maximum amount of concurrent connections is already occupied.
        NoMoreConnections = 0x24,
        // The KNXnet/IP Server device detects an error concerning the data connection with the specified ID. 
        DataConnection = 0x26,
        // The KNXnet/IP Server device detects an error  concerning the KNX subnetwork connection with the specified ID. 
        KnxConnection = 0x27,
        // The Client is not authorised to use the requested IA in the Extended CRI. 
        AuthorisationError = 0x28,
        // The requested tunnelling layer is not supported by the KNXnet/IP Server device.
        TunnelingLayer = 0x29,
        // The IA requested in the Extended CRI is not a Tunnelling IA. 
        NoTunnelingAddress = 0x2D,
        // The IA requested for this connection is in use.
        ConnectionInUse = 0x2E,
    }
}
