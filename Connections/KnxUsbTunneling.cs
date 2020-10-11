using Device.Net;
using Hid.Net;
using Kaenx.Konnect.Builders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Connections
{
    public class KnxUsbTunneling : IKnxConnection
    {
        public bool IsConnected { get; set; }

        public event IKnxConnection.TunnelRequestHandler OnTunnelRequest;
        public event IKnxConnection.TunnelRequestHandler OnTunnelResponse;
        public event IKnxConnection.TunnelRequestHandler OnTunnelAck;
        public event IKnxConnection.SearchResponseHandler OnSearchResponse;
        public event IKnxConnection.ConnectionChangedHandler ConnectionChanged;

        private string DeviceId { get; }
        private IDevice DeviceKnx { get; }

        public KnxUsbTunneling(string deviceId)
        {
            DeviceId = deviceId;
            DeviceKnx = DeviceManager.Current.GetDevice(new ConnectedDeviceDefinition(deviceId));
        }


        public async Task Connect()
        {
            byte[] packet = new byte[63]; //Create packet which will be fixed 64 bytes long

            //Report Header
            packet[0] = 0x13; // First Packet with start and end
            packet[1] = 9; // Data length


            //Transfer Header
            packet[2] = 0; // Version fixed 0
            packet[3] = 8; // Header length
            packet[4] = 0; // Body length
            packet[5] = 1; // Body length
            packet[6] = 0x0F; // Protocol ID
            packet[7] = 0x01; // Service Identifier
            packet[8] = 0; // Manufacturer Code
            packet[9] = 0; // Manufacturer Code

            // Body
            packet[10] = 1; // Bus Status

            await DeviceKnx.InitializeAsync();

            Device.Net.ReadResult read = await DeviceKnx.WriteAndReadAsync(packet);

            IsConnected = true;
            ConnectionChanged?.Invoke(true);
        }

        public async Task Disconnect()
        {
            DeviceKnx.Dispose();
        }

        public async Task Send(byte[] data, bool ignoreConnected = false)
        {
            throw new NotImplementedException();
        }

        public async Task<byte> Send(IRequestBuilder builder, bool ignoreConnected = false)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SendStatusReq()
        {
            byte[] packet = new byte[64]; //Create packet which will be fixed 64 bytes long

            //Report Header
            packet[0] = 1; // Report ID fixed 1;
            packet[1] = 0x13; // First Packet with start and end
            packet[2] = 9; // Data length


            //Transfer Header
            packet[3] = 0; // Version fixed 0
            packet[4] = 8; // Header length
            packet[5] = 0; // Body length
            packet[6] = 1; // Body length
            packet[7] = 0x0F; // Protocol ID
            packet[8] = 0x01; // Service Identifier
            packet[9] = 0; // Manufacturer Code
            packet[10] = 0; // Manufacturer Code

            // Body
            packet[11] = 3; // Bus Status

            await DeviceKnx.InitializeAsync();

            await DeviceKnx.WriteAsync(packet);

            DeviceKnx.Close();


            return true;
        }
    }
}
