using Device.Net;
using Hid.Net;
using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Messages;
using Kaenx.Konnect.Messages.Request;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Kaenx.Konnect.Connections.IKnxConnection;

namespace Kaenx.Konnect.Connections
{
    public class KnxUsbTunneling : IKnxConnection
    {
        public bool IsConnected { get; set; }

        public event TunnelRequestHandler OnTunnelRequest;
        public event TunnelResponseHandler OnTunnelResponse;
        public event TunnelAckHandler OnTunnelAck;
        public event SearchResponseHandler OnSearchResponse;
        public event ConnectionChangedHandler ConnectionChanged;

        private string DeviceId { get; }
        private IDevice DeviceKnx { get; }
        private ProtocolTypes CurrentType { get; set; }
        public ConnectionErrors LastError { get; set; }
        public UnicastAddress PhysicalAddress { get; set; }


        public KnxUsbTunneling(string deviceId)
        {
            DeviceId = deviceId;
            DeviceKnx = DeviceManager.Current.GetDevice(new ConnectedDeviceDefinition(deviceId));
        }


        //TODO Adresse der Schnitstelle schreibt man im Speicher an Adresse 279

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

            bool myflag = false;
            CancellationTokenSource source = new CancellationTokenSource(1000);


            _ = Task.Run(async () =>
            {
                Device.Net.ReadResult read = await DeviceKnx.WriteAndReadAsync(packet);
                myflag = true;
            }, source.Token);

            while(!myflag && !source.IsCancellationRequested)
            {
                await Task.Delay(100);
            }

            if (!myflag)
            {
                DeviceKnx.Close();
                return;
            }

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

        public async Task<byte> Send(IMessageRequest message, bool ignoreConnected = false)
        {
            if (!ignoreConnected && !IsConnected)
                throw new Exception("Roflkopter");

            //var seq = _sequenceCounter;
            //message.SetInfo(_communicationChannel, _sequenceCounter);
            //_sequenceCounter++;

            byte[] data;

            switch (CurrentType)
            {
                case ProtocolTypes.Emi1:
                    data = message.GetBytesEmi1();
                    break;

                case ProtocolTypes.Emi2:
                    data = message.GetBytesEmi2();
                    break;

                case ProtocolTypes.cEmi:
                    data = message.GetBytesCemi();
                    break;

                default:
                    throw new Exception("Unbekanntes Protokoll");
            }

            await DeviceKnx.WriteAsync(data);

            return 0x01; // return seq
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
