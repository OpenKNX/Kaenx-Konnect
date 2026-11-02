using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Connections.Transports;
using Kaenx.Konnect.EMI;
using Kaenx.Konnect.EMI.LData;
using Kaenx.Konnect.Telegram.IP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Connections.Protocols
{
    public abstract class IProtocol : IDisposable
    {
        internal ITransport _transport;

        public delegate void ReceivedService(IpTelegram ipTelegram);
        public event ReceivedService? OnReceivedService;

        public delegate void ReceivedMessage(LDataBase message);
        public event ReceivedMessage? OnReceivedMessage;

        public virtual bool IsConnected { get; set; } = false;
        public abstract UnicastAddress? LocalAddress { get; }

        internal IProtocol(ITransport connection)
        {
            _transport = connection;
        }

        public IPEndPoint GetLocalEndpoint()
        {
            return _transport.GetLocalEndpoint();
        }

        public void InvokeReceivedService(IpTelegram ipTelegram)
        {
            OnReceivedService?.Invoke(ipTelegram);
        }

        public void InvokeReceivedMessage(LDataBase message)
        {
            OnReceivedMessage?.Invoke(message);
        }

        public abstract Task Connect();

        public virtual async Task Disconnect()
        {
            // TODO get real IPEndpoint
            //IPEndPoint localEndpoint = new IPEndPoint(IPAddress.Parse("192.168.178.84"), GetLocalEndpoint().Port);
            //DisconnectRequest dreq = new DisconnectRequest(ChannelId, localEndpoint, Kaenx.Konnect.Telegram.Enums.HostProtocols.IPv4_UDP);
            //await SendAsync(dreq);
        }

        public virtual async Task SendRawAsync(byte[] data)
        {
            await _transport.SendAsync(data);
        }

        public abstract Task SendAsync(IpTelegram ipTelegram);
        public abstract Task<int> SendAsync(LDataBase message);

        public void Dispose()
        {
            _transport.Dispose();
        }

        public virtual int GetMaxApduLength()
        {
            return 250;
        }
    }
}
