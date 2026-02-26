using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Connections;
using Kaenx.Konnect.EMI.DataMessages;
using Kaenx.Konnect.EMI.LData;
using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Classes
{
    public class BusCommon
    {
        private IKnxConnection _conn;
        private UnicastAddress from = UnicastAddress.FromString("0.0.0");
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
            var content = new GroupValueWrite(data);
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
            // TODO: IndividualAddressRead IDataMessage fehlt noch im Repo
            await Task.CompletedTask;
        }

        public async Task IndividualAddressWrite(UnicastAddress newAddr)
        {
            // TODO: IndividualAddressWrite IDataMessage fehlt noch im Repo
            await Task.CompletedTask;
        }

        public async Task IndividualAddressWrite(UnicastAddress newAddr, byte[] serialNumber)
        {
            // TODO: IndividualAddressSerialWrite IDataMessage fehlt noch im Repo
            await Task.CompletedTask;
        }

        public async Task<LDataBase> ReadSerialNumberByManufacturer(int manufacturerId)
        {
            // TODO: SystemNetworkParameterRead IDataMessage fehlt noch im Repo
            byte[] data = BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder((short)manufacturerId));
            await Task.CompletedTask;
            return null!;
        }
    }
}