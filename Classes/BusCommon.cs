using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Connections;
using Kaenx.Konnect.Messages.Request;
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



        public async void IndividualAddressRead()
        {
            MsgIndividualAddressRead message = new MsgIndividualAddressRead();
            await _conn.Send(message);
        }

        public async  void IndividualAddressWrite(UnicastAddress newAddr)
        {
            MsgIndividualAddressWrite message = new MsgIndividualAddressWrite(newAddr);
            await _conn.Send(message);
        }


        public async void IndividualAddressWrite(UnicastAddress newAddr, byte[] serialNumber)
        {
            MsgIndividualAddressSerialWrite message = new MsgIndividualAddressSerialWrite(newAddr, serialNumber);
            await _conn.Send(message);
        }



        public async void GroupValueWrite(MulticastAddress ga, byte[] data)
        {
            MsgGroupValueWrite message = new MsgGroupValueWrite(from, ga, data);
            await _conn.Send(message);
        }

        public async Task GroupValueRead(MulticastAddress ga)
        {
            MsgGroupValueRead message = new MsgGroupValueRead(ga);
            await _conn.Send(message);
            var x = await WaitForData(lastReceivedNumber);
        }
    }
}
