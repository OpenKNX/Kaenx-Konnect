using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Connections;
using Kaenx.Konnect.EMI.DataMessages;
using Kaenx.Konnect.EMI.LData;
using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task GroupValueWrite(MulticastAddress ga, bool value)
        {
            byte[] payload = new byte[] { value ? (byte)0x01 : (byte)0x00 };
            var content = new GroupValueWrite(payload);
            LDataBase message = new LDataBase(ga, false, 0, content);
            await _conn.SendAsync(message);
        }

        public async Task GroupValueWrite(MulticastAddress ga, byte value)
        {
            byte[] payload = new byte[] { 0x00, value };
            var content = new GroupValueWrite(payload);
            LDataBase message = new LDataBase(ga, false, 0, content);
            await _conn.SendAsync(message);
        }

        public async Task GroupValueWrite(MulticastAddress ga, byte[] data)
        {
            var content = new GroupValueWrite(data);
            LDataBase message = new LDataBase(ga, false, 0, content);
            await _conn.SendAsync(message);
        }

        public async Task<LDataBase?> GroupValueRead(MulticastAddress ga)
        {
            TaskCompletionSource<LDataBase> tcs = new TaskCompletionSource<LDataBase>();

            void ReceivedMessage(LDataBase response)
            {
                if (response.GetApciType() == ApciTypes.GroupValueResponse &&
                    response.DestinationAddress is MulticastAddress destGa &&
                    destGa.GetBytes().SequenceEqual(ga.GetBytes()))
                {
                    tcs.TrySetResult(response);
                }
            }

            _conn.OnReceivedMessage += ReceivedMessage;

            try
            {
                var content = new GroupValueRead();
                LDataBase message = new LDataBase(ga, false, 0, content);
                await _conn.SendAsync(message);

                if (await Task.WhenAny(tcs.Task, Task.Delay(3000)) != tcs.Task)
                    return null;

                return await tcs.Task;
            }
            finally
            {
                _conn.OnReceivedMessage -= ReceivedMessage;
            }
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