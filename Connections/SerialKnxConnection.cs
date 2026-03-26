using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Connections.Transports;
using Kaenx.Konnect.EMI.LData;
using Kaenx.Konnect.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Connections
{
    internal class SerialKnxConnection : IKnxConnection
    {
        private readonly SerialTransport _transport;
        private readonly CancellationTokenSource _cts = new();
        private readonly byte[] _buffer = new byte[2048];
        private int _bufferIndex = 0;

        public bool IsConnected => true; // USB ist immer "connected"

        public event IKnxConnection.ReceivedMessage? OnReceivedMessage;
        public event IKnxConnection.ReceivedService? OnReceivedService;

        public SerialKnxConnection(SerialTransport connection)
        {
            _transport = connection;
            _transport.OnReceived += Transport_OnReceived;
        }

        private async Task Transport_OnReceived(object sender, byte[] data)
        {
            Array.Copy(data, 0, _buffer, _bufferIndex, data.Length);
            _bufferIndex += data.Length;

            TryParseFrames();
        }

        public Task Connect()
        {
            // SerialTransport öffnet den Port bereits im Konstruktor
            return Task.CompletedTask;
        }

        public Task Disconnect()
        {
            _cts.Cancel();
            return Task.CompletedTask;
        }

        public int GetMaxApduLength() => 15;

        public UnicastAddress? GetLocalAddress() => null;

        public async Task<int> SendAsync(LDataBase message)
        {
            byte[] cemi = message.GetBytesCemi();
            await SendAsync(cemi);
            return cemi.Length;
        }

        public Task SendAsync(byte[] data)
        {
            return _transport.SendAsync(data);
        }

 

        private void TryParseFrames()
        {
            int idx = 0;

            while (idx + 2 < _bufferIndex)
            {
                // FT1.2 Startbyte
                if (_buffer[idx] != 0x68)
                {
                    idx++;
                    continue;
                }

                int len = _buffer[idx + 1];

                // FT1.2: Start(1) + Len(1) + Len(1) + Payload + Checksumme(1) + End(1)
                int frameLength = len + 6;

                if (idx + frameLength > _bufferIndex)
                    break; // Frame noch nicht vollständig

                byte[] frame = new byte[frameLength];
                Array.Copy(_buffer, idx, frame, 0, frameLength);

                HandleFrame(frame);

                idx += frameLength;
            }

            // Rest nach vorne schieben
            if (idx > 0)
            {
                Array.Copy(_buffer, idx, _buffer, 0, _bufferIndex - idx);
                _bufferIndex -= idx;
            }
        }

        private void HandleFrame(byte[] frame)
        {
            // cEMI beginnt bei Byte 4
            if (frame.Length < 10)
                return;

            byte msgCode = frame[4];

            switch (msgCode)
            {
                case 0x29: // L_Data.ind
                case 0x2E: // L_Data.con
                case 0x2F: // L_Data.req
                    var ldata = new LDataBase(frame.AsSpan(4).ToArray(), ExternalMessageInterfaces.cEmi);
                    OnReceivedMessage?.Invoke( ldata);
                    break;

                default:
                   // OnReceivedService?.Invoke(this, frame);
                    break;
            }
        }
    }
}
