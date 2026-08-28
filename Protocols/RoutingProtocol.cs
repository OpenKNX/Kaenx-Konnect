using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Connections.Transports;
using Kaenx.Konnect.EMI;
using Kaenx.Konnect.EMI.LData;
using Kaenx.Konnect.Enums;
using Kaenx.Konnect.Telegram.Contents;
using Kaenx.Konnect.Telegram.IP;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Connections.Protocols
{
    internal class RoutingProtocol : IProtocol
    {
        public override bool IsConnected => true;

        private UnicastAddress? _sourceAddress;
        public override UnicastAddress? LocalAddress => _sourceAddress;

        public RoutingProtocol(UnicastAddress sourceAddress, ITransport connection)
            : base(connection)
        {
            connection.OnReceived += Connection_OnReceived;
            _sourceAddress = sourceAddress;
        }

        private Task Connection_OnReceived(object sender, byte[] data)
        {
            try
            {
                KnxIpHeader header = new KnxIpHeader(data);

                switch (header.ServiceIdentifier)
                {
                    case ServiceIdentifiers.RoutingIndication:
                        {
                            byte[] cemi = data.Skip(header.HeaderLength).ToArray();
                            EmiContent content = new EmiContent(cemi, ExternalMessageInterfaces.cEmi);
                            LDataBase? lDataBase = content.Message as LDataBase;
                            if (lDataBase == null) break;
                            InvokeReceivedMessage(lDataBase);
                            break;
                        }

                    default:
                        Debug.WriteLine($"[RoutingProtocol] Unhandled ServiceIdentifier: {data[2]:X2}{data[3]:X2}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RoutingProtocol] Error in Connection_OnReceived: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        public override async Task SendAsync(IpTelegram ipTelegram)
        {
            await _transport.SendAsync(ipTelegram.ToByteArray());
        }

        public override async Task<int> SendAsync(LDataBase message)
        {
            // Routing Indication manuell bauen:
            // KNX IP Header (6 Bytes) + cEMI
            EmiContent emiContent = new EmiContent(message, ExternalMessageInterfaces.cEmi);
            byte[] cemi = emiContent.ToByteArray();

            // Header: 06 10 05 30 [length hi] [length lo]
            int totalLength = 6 + cemi.Length;
            byte[] packet = new byte[totalLength];
            packet[0] = 0x06; // Header length
            packet[1] = 0x10; // KNX IP version
            packet[2] = 0x05; // Service ID hi (RoutingIndication)
            packet[3] = 0x30; // Service ID lo
            packet[4] = (byte)(totalLength >> 8);
            packet[5] = (byte)(totalLength & 0xFF);
            Array.Copy(cemi, 0, packet, 6, cemi.Length);

            await _transport.SendAsync(packet);
            return 0;
        }

        public override Task Connect()
        {
            return Task.CompletedTask;
        }
    }
}