using Device.Net;
using Kaenx.Konnect.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;

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


        public async void Connect()
        {
            var deviceDefinitions = new List<FilterDeviceDefinition>
            {
                new FilterDeviceDefinition{ DeviceType= DeviceType.Hid, UsagePage = 0xFFA0, Label="Trezor One Firmware 1.6.x" }
            };

            var devices = await DeviceManager.Current.GetDevicesAsync(deviceDefinitions);



            //HidDevice dev = await HidDevice.FromIdAsync(devices[1].Id, FileAccessMode.ReadWrite);
            //dev.InputReportReceived += Dev_InputReportReceived;
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void Send(byte[] data)
        {
            throw new NotImplementedException();
        }

        public byte Send(IRequestBuilder builder)
        {
            throw new NotImplementedException();
        }

        public void SendStatusReq()
        {
            throw new NotImplementedException();
        }
    }
}
