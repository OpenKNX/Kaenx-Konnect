using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Connections.Protocols;
using Kaenx.Konnect.EMI.LData;
using Kaenx.Konnect.Enums;
using Kaenx.Konnect.Telegram.IP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Connections
{
    public class IpKnxConnection : IKnxConnection
    {
        private IProtocol _protocol;

        public event IKnxConnection.ReceivedMessage? OnReceivedMessage;
        public event IKnxConnection.ReceivedService? OnReceivedService;

        internal IpKnxConnection(IProtocol protocol)
        {
            string name = protocol.GetType().Name;
            if (name != "TunnelingProtocol")
                throw new Exception("Cant create IpKnxCOnnection from protocol: " + name);
            _protocol = protocol;

            _protocol.OnReceivedMessage += _protocol_OnReceivedMessage;
            _protocol.OnReceivedService += _protocol_OnReceivedService;
        }

        private void _protocol_OnReceivedMessage(LDataBase message)
        {
            OnReceivedMessage?.Invoke(message);
        }

        private void _protocol_OnReceivedService(IpTelegram ipTelegram)
        {
            OnReceivedService?.Invoke(ipTelegram);
        }

        public IPEndPoint GetLocalEndpoint()
        {
            return _protocol.GetLocalEndpoint();
        }

        public async Task SendAsync(IpTelegram ipTelegram)
        {
            await _protocol.SendAsync(ipTelegram);
        }

        public async Task<int> SendAsync(LDataBase message)
        {
            return await _protocol.SendAsync(message);
        }

        public async Task SendAsync(byte[] data)
        {
            await _protocol.SendRawAsync(data);
        }

        public async Task Connect()
        {
            await _protocol.Connect();
        }

        public async Task Disconnect()
        {
            await _protocol.Disconnect();
        }

        public void Dispose()
        {
            _protocol.OnReceivedMessage -= _protocol_OnReceivedMessage;
            _protocol.Dispose();
        }

        public int GetMaxApduLength()
        {
            return _protocol.GetMaxApduLength();
        }

        public UnicastAddress? GetLocalAddress()
        {
            return _protocol.LocalAddress;
        }

        public uint GetChannelId()
        {
            if (_protocol is TunnelingProtocol tp)
            {
                return tp.GetChannelId();
            }
            throw new Exception("Cant get ChannelId from protocol: " + _protocol.GetType().Name);
        }

        public HostProtocols GetHostProtocol()
        {
            if(_protocol is TunnelingProtocol tp)
            {
                return tp.GetProtocolType();
            }
            throw new Exception("Cant get ChannelId from protocol: " + _protocol.GetType().Name);
        }
    }
}
