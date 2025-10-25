using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Connections.Connections;
using Kaenx.Konnect.EMI.LData;
using Kaenx.Konnect.Messages.Request;
using Kaenx.Konnect.Messages.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Classes
{
    public class BusCommon
    {
        private IKnxConnection _conn;
        private MulticastAddress to = MulticastAddress.FromString("0/0/0");
        private UnicastAddress from = UnicastAddress.FromString("0.0.0");
        private Dictionary<int, LDataBase> responses = new Dictionary<int, LDataBase>();

        private int _lastNumb = -1;
        private int lastReceivedNumber
        {
            get { return _lastNumb == 15 ? 0 : _lastNumb + 1; }
            set { _lastNumb = value; }
        }


        public BusCommon(IKnxConnection conn)
        {
            _conn = conn;
            //_conn.OnTunnelResponse += _conn_OnTunnelResponse;
        }

        private void _conn_OnTunnelResponse(LDataBase response)
        {
            if (responses.ContainsKey(response.SequenceNumber))
                responses[response.SequenceNumber] = response;
            else
                responses.Add(response.SequenceNumber, response);

            lastReceivedNumber = response.SequenceNumber;
        }


        private async Task<LDataBase> WaitForData(int seq)
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
            // TODO
            //await _conn.SendAsync(message, to);
            await Task.Delay(200);
        }

        public async Task IndividualAddressWrite(UnicastAddress newAddr)
        {
            MsgIndividualAddressWriteReq message = new MsgIndividualAddressWriteReq(newAddr);
            // TODO
            //await _conn.SendAsync(message, to);
            await Task.Delay(200);
        }


        public async Task IndividualAddressWrite(UnicastAddress newAddr, byte[] serialNumber)
        {
            MsgIndividualAddressSerialWriteReq message = new MsgIndividualAddressSerialWriteReq(newAddr, serialNumber);
            // TODO
            //await _conn.SendAsync(message, to);
            await Task.Delay(200);
        }

        public async Task<LDataBase> ReadSerialNumberByManufacturer(int manufacturerId)
        {
            byte[] data = BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder((short)manufacturerId));

            MsgSystemNetworkParameterReadReq message = new MsgSystemNetworkParameterReadReq(MsgSystemNetworkParameterReadOperand.ByManufacturerSpecific, data);
            // TODO
            var seq = 0; // await _conn.SendAsync(message, to);
            var x = await WaitForData(seq);
            return x;
        }



        public async Task GroupValueWrite(MulticastAddress ga, byte data)
        {
            await GroupValueWrite(ga, new byte[] { data });
        }

        public async Task GroupValueWrite(MulticastAddress ga, byte[] data)
        {
            MsgGroupWriteReq message = new MsgGroupWriteReq(from, ga, data);
            // TODO
            //await _conn.SendAsync(message, to);
        }

        public async Task<LDataBase> GroupValueRead(MulticastAddress ga)
        {
            MsgGroupReadReq message = new MsgGroupReadReq(ga);
            // TODO
            var seq = 0; // await _conn.SendAsync(message, to);
            var x = await WaitForData(seq);
            return x;
        }
    }
}
