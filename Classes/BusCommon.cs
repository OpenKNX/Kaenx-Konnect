using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Classes
{
    public class BusCommon
    {
        private IKnxConnection _conn;
        private MulticastAddress to = MulticastAddress.FromString("0/0/0");
        private UnicastAddress from = UnicastAddress.FromString("0.0.0");
        private Dictionary<int, TunnelResponse> responses = new Dictionary<int, TunnelResponse>();

        private int _lastNumb = -1;
        private int lastReceivedNumber
        {
            get { return _lastNumb == 15 ? 0 : _lastNumb + 1; }
            set { _lastNumb = value; }
        }


        public BusCommon(IKnxConnection conn)
        {
            _conn = conn;
            _conn.OnTunnelResponse += _conn_OnTunnelResponse;
        }

        private void _conn_OnTunnelResponse(TunnelResponse response)
        {
            if (responses.ContainsKey(response.SequenceNumber))
                responses[response.SequenceNumber] = response;
            else
                responses.Add(response.SequenceNumber, response);

            lastReceivedNumber = response.SequenceNumber;
        }


        private async Task<TunnelResponse> WaitForData(int seq)
        {
            if (responses.ContainsKey(seq))
                responses.Remove(seq);

            while (!responses.ContainsKey(seq))
                await Task.Delay(10); // TODO maybe erhöhen

            var resp = responses[seq];
            responses.Remove(seq);
            return resp;
        }



        public void IndividualAddressRead()
        {
            TunnelRequest builder = new TunnelRequest();
            builder.Build(from, to, Parser.ApciTypes.IndividualAddressRead);
            _conn.Send(builder);
        }

        public void IndividualAddressWrite(UnicastAddress newAddr)
        {
            TunnelRequest builder = new TunnelRequest();
            builder.Build(MulticastAddress.FromString("0/0/0"), MulticastAddress.FromString("0/0/0"), Parser.ApciTypes.IndividualAddressWrite, 255, newAddr.GetBytes());
            builder.SetPriority(Prios.System);
            _conn.Send(builder);
        }


        public void IndividualAddressWrite(UnicastAddress newAddr, byte[] serialNumber)
        {
            TunnelRequest builder = new TunnelRequest();

            List<byte> data = new List<byte>();
            data.AddRange(serialNumber);

            data.AddRange(newAddr.GetBytes());
            data.AddRange(new byte[] { 0, 0, 0, 0 });

            builder.Build(MulticastAddress.FromString("0/0/0"), MulticastAddress.FromString("0/0/0"), Parser.ApciTypes.IndividualAddressSerialNumberWrite, 255, data.ToArray());
            builder.SetPriority(Prios.System);
            _conn.Send(builder);
        }



        public void GroupValueWrite(MulticastAddress ga, byte[] data)
        {
            TunnelRequest builder = new TunnelRequest();
            builder.Build(from, ga, Parser.ApciTypes.GroupValueWrite, 255, data);
            _conn.Send(builder);
        }

        public async Task GroupValueRead(MulticastAddress ga)
        {
            TunnelRequest builder = new TunnelRequest();
            builder.Build(from, ga, Parser.ApciTypes.GroupValueRead);
            _conn.Send(builder);
            var x = await WaitForData(lastReceivedNumber);
        }
    }
}
