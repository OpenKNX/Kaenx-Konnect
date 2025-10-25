using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Enums
{
    public enum ServiceIdentifiers
    {
        Unknown = 0x0000,
        SearchRequest = 0x0201,
        SearchResponse = 0x0202,
        DescriptionRequest = 0x0203,
        DescriptionResponse = 0x0204,
        ConnectRequest = 0x0205,
        ConnectResponse = 0x0206,
        ConnectionStateRequest = 0x0207,
        ConnectionStateResponse = 0x0208,
        DisconnectRequest = 0x0209,
        DisconnectResponse = 0x020A,
        DeviceConfigurationRequest = 0x0310,
        DeviceConfigurationAck = 0x0311,
        TunnelingRequest = 0x0420,
        TunnelingAck = 0x0421,
        TunnelFeatureGet = 0x0422,
        TunnelFeatureResponse = 0x0423,
        TunnelFeatureSet = 0x0424,
        RoutingIndication = 0x0530,
        RoutingLostMessage = 0x0531,
    }
}
