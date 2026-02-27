using System;
using System.Collections.Generic;
using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Connections;
using Kaenx.Konnect.EMI.DataMessages;
using Kaenx.Konnect.EMI.LData;
using System.Net;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Classes
{
    public class BusCommon
    {
        private IKnxConnection _conn;
        private Dictionary<byte, LDataBase> responses = new Dictionary<byte, LDataBase>();

        public BusCommon(IKnxConnection conn)
        {
            _conn = conn;
            _conn.OnReceivedMessage += OnReceivedMessage;
        }

        private void OnReceivedMessage(LDataBase response)
        {
            responses[response.SequenceNumber] = response;
        }

        private async Task<LDataBase> WaitForData(byte seq)
        {
            responses.Remove(seq);
            while (!responses.ContainsKey(seq))
                await Task.Delay(10);
            var resp = responses[seq];
            responses.Remove(seq);
            return resp;
        }

        public async Task GroupValueWrite(MulticastAddress ga, byte data)
        {
            await GroupValueWrite(ga, new byte[] { data });
        }

        public async Task GroupValueWrite(MulticastAddress ga, byte[] data)
        {
            // DPT 5.x (1 Byte) send as 2-Byte-Array - trick stolen from xknx 
            byte[] payload = data.Length == 1 ? new byte[] { 0x00, data[0] } : data;
            var content = new GroupValueWrite(payload);
            LDataBase message = new LDataBase(ga, false, 0, content);
            await _conn.SendAsync(message);
        }

        public async Task<LDataBase> GroupValueRead(MulticastAddress ga)
        {
            var content = new GroupValueRead();
            LDataBase message = new LDataBase(ga, false, 0, content);
            int seq = await _conn.SendAsync(message);
            return await WaitForData((byte)seq);
        }

        public async Task IndividualAddressRead()
        {
            await Task.CompletedTask;
        }

        public async Task IndividualAddressWrite(UnicastAddress newAddr)
        {
            await Task.CompletedTask;
        }

        public async Task IndividualAddressWrite(UnicastAddress newAddr, byte[] serialNumber)
        {
            await Task.CompletedTask;
        }

        public async Task<LDataBase> ReadSerialNumberByManufacturer(int manufacturerId)
        {
            byte[] data = BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder((short)manufacturerId));
            await Task.CompletedTask;
            return null!;
        }
    }
}