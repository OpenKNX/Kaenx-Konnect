using Device.Net;
using Kaenx.Konnect.Builders;
using System;
using System.Collections.Generic;
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
        private IDevice Device { get; }

        public KnxUsbTunneling(string deviceId)
        {
            DeviceId = deviceId;
            Device = DeviceManager.Current.GetDevice(new ConnectedDeviceDefinition(deviceId));
        }


        public async Task Connect()
        {
            var deviceDefinitions = new List<FilterDeviceDefinition>
            {
                new FilterDeviceDefinition{ DeviceType= DeviceType.Hid, UsagePage = 0xFFA0, Label="Trezor One Firmware 1.6.x" }
            };

            var devices = await DeviceManager.Current.GetDevicesAsync(deviceDefinitions);



            //HidDevice dev = await HidDevice.FromIdAsync(devices[1].Id, FileAccessMode.ReadWrite);
            //dev.InputReportReceived += Dev_InputReportReceived;
        }

        public async Task Disconnect()
        {
            throw new NotImplementedException();
        }

        public async Task Send(byte[] data)
        {
            throw new NotImplementedException();
        }

        public async Task<byte> Send(IRequestBuilder builder)
        {
            throw new NotImplementedException();
        }

        public async Task SendStatusReq()
        {
            byte[] packet = new byte[64]; //Create packet which will be fixed 64 bytes long

            //Report Header
            packet[0] = 1; // Report ID fixed 1;
            packet[1] = 0x13; // First Packet with start and end
            packet[2] = 9; // Data length


            //Transfer Header
            packet[3] = 0; // Version fixed 0
            packet[4] = 8; // Header length
            packet[5] = 1; // Body length
            packet[6] = 0x0F; // Protocol ID
            packet[7] = 0x01; // Service Identifier
            packet[8] = 0; // Manufacturer Code

            // Body
            packet[9] = 3; // Bus Status

            ReadResult x = await Device.WriteAndReadAsync(packet);

        }
    }
}
