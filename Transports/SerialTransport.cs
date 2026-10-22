using Kaenx.Konnect.Enums;
using System.IO.Ports;
using System.Net;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Connections.Transports
{
    internal class SerialTransport : ITransport
    {
        private readonly SerialPort _port;

        public bool IsAckRequired => false; 

        public event ITransport.ReceivedKnxMessage? OnReceived;

        public SerialTransport(string portName, int baudRate = 19200)
        {
            _port = new SerialPort(portName, baudRate, Parity.Even, 8, StopBits.One);
            _port.Handshake = Handshake.None;
            _port.DataReceived += Port_DataReceived;
            _port.Open();
        }

        public void Dispose()
        {
            if (_port.IsOpen)
                _port.Close();

            _port.Dispose();
        }

        public HostProtocols GetProtocolType()
            => HostProtocols.Serial;

        public IPEndPoint GetLocalEndpoint()
            => new IPEndPoint(IPAddress.None, 0);

        public Task SendAsync(byte[] data)
        {
            _port.Write(data, 0, data.Length);
            return Task.CompletedTask;
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytes = _port.BytesToRead;
            if (bytes <= 0) return;

            byte[] buffer = new byte[bytes];
            _port.Read(buffer, 0, bytes);

            OnReceived?.Invoke(this, buffer);
        }
    }
}
