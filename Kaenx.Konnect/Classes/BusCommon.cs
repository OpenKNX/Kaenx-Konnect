using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Connections;
using Kaenx.Konnect.Messages.Request;
using Kaenx.Konnect.Messages.Response;
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
        private Dictionary<int, IMessageResponse> responses = new Dictionary<int, IMessageResponse>();

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

        private void _conn_OnTunnelResponse(IMessageResponse response)
        {
            if (responses.ContainsKey(response.SequenceNumber))
                responses[response.SequenceNumber] = response;
            else
                responses.Add(response.SequenceNumber, response);

            lastReceivedNumber = response.SequenceNumber;
        }


        private async Task<IMessageResponse> WaitForData(int seq)
        {
            if (responses.ContainsKey(seq))
                responses.Remove(seq);

            while (!responses.ContainsKey(seq))
                await Task.Delay(10); // TODO maybe erhöhen

            var resp = responses[seq];
            responses.Remove(seq);
            return resp;
        }



        public async Task IndividualAddressRead()
        {
            MsgIndividualAddressReadReq message = new MsgIndividualAddressReadReq();
            await _conn.Send(message);
            await Task.Delay(200);
        }

        public async Task IndividualAddressWrite(UnicastAddress newAddr)
        {
            MsgIndividualAddressWriteReq message = new MsgIndividualAddressWriteReq(newAddr);
            await _conn.Send(message);
            await Task.Delay(200);
        }


        public async Task IndividualAddressWrite(UnicastAddress newAddr, byte[] serialNumber)
        {
            MsgIndividualAddressSerialWriteReq message = new MsgIndividualAddressSerialWriteReq(newAddr, serialNumber);
            await _conn.Send(message);
            await Task.Delay(200);
        }



        public async void GroupValueWrite(MulticastAddress ga, byte[] data)
        {
            MsgGroupWriteReq message = new MsgGroupWriteReq(from, ga, data);
            await _conn.Send(message);
        }

        public async Task<IMessageResponse> GroupValueRead(MulticastAddress ga)
        {
            MsgGroupReadReq message = new MsgGroupReadReq(ga);
            var seq = await _conn.Send(message);
            var x = await WaitForData(seq);
            return x;
        }
    }
}
