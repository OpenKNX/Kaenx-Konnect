using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Connections;
using Kaenx.Konnect.EMI.DataMessages;
using Kaenx.Konnect.EMI.LData;
using Kaenx.Konnect.Enums;
using Kaenx.Konnect.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public async Task<bool> GetProgrammingMode(UnicastAddress address)
        {
            byte result = await MemoryReadByte(address, 0x0060);
            return (result & 0x01) != 0;
        }

        public async Task SetProgrammingMode(UnicastAddress address, bool enable)
        {
            byte current = await MemoryReadByte(address, 0x0060);
            byte newValue = enable
                ? (byte)(current | 0x01)
                : (byte)(current & 0xFE);
            await MemoryWriteByte(address, 0x0060, newValue);
        }

        public async Task<bool> ToggleProgrammingMode(UnicastAddress address)
        {
            bool current = await GetProgrammingMode(address);
            await SetProgrammingMode(address, !current);
            return !current;
        }

        private async Task<byte> MemoryReadByte(UnicastAddress address, ushort memAddress)
        {
            UnicastAddress local = _conn.GetLocalAddress()
                ?? UnicastAddress.FromByteArray(new byte[] { 0xFF, 0xFF });

            var ddTcs = new TaskCompletionSource<IDataMessage>();
            var memTcs = new TaskCompletionSource<IDataMessage>();

            void OnMessage(LDataBase msg)
            {
                Debug.WriteLine($"[MemRead] ← {msg.Content?.GetType().Name ?? "Control"}, IsControl={msg.IsControl}, Numbered={msg.IsNumbered}, Seq={msg.SequenceNumber}, Raw={Convert.ToHexString(msg.RawBytes)}");

                if (msg.IsControl || msg.Content == null) return;

                // Echo des eigenen Requests ignorieren — endet nicht auf "Response"
                if (!msg.Content.GetType().Name.EndsWith("Response")) return;

                if (msg.Content is DeviceDescriptorResponse)
                    ddTcs.TrySetResult(msg.Content);
                else if (msg.Content is MemoryResponse)
                    memTcs.TrySetResult(msg.Content);
            }

            _conn.OnReceivedMessage += OnMessage;

            try
            {
                // 1. T_Connect
                await _conn.SendAsync(new LDataBase(address, false, 0, new Connect()));
                await Task.Delay(150);

                // 2. DeviceDescriptor_Read (S=0)
                await _conn.SendAsync(new LDataBase(address, true, 0, new DeviceDescriptorRead()));

                if (await Task.WhenAny(ddTcs.Task, Task.Delay(3000)) != ddTcs.Task)
                    Debug.WriteLine("[MemRead] DeviceDescriptor Timeout — fahre trotzdem fort");

                await Task.Delay(150);

                // 3. Memory_Read (S=1)
                await _conn.SendAsync(new LDataBase(address, true, 1, new MemoryRead(memAddress, 1)));

                if (await Task.WhenAny(memTcs.Task, Task.Delay(5000)) != memTcs.Task)
                    throw new TimeoutException($"Keine MemoryResponse von {address}");

                return ((MemoryResponse)await memTcs.Task).Data[0];
            }
            finally
            {
                await _conn.SendAsync(new LDataBase(address, false, 0, new Disconnect()));
                _conn.OnReceivedMessage -= OnMessage;
            }
        }

        private async Task MemoryWriteByte(UnicastAddress address, ushort memAddress, byte value)
        {
            UnicastAddress local = _conn.GetLocalAddress()
                ?? UnicastAddress.FromByteArray(new byte[] { 0xFF, 0xFF });

            var ddTcs = new TaskCompletionSource<IDataMessage>();
            var readTcs = new TaskCompletionSource<IDataMessage>();
            var writeTcs = new TaskCompletionSource<IDataMessage>();
            bool readDone = false;

            void OnMessage(LDataBase msg)
            {
                if (msg.IsControl || msg.Content == null) return;
                Debug.WriteLine($"[MemWrite] ← {msg.Content.GetType().Name}, Seq={msg.SequenceNumber}");

                if (msg.IsNumbered && msg.SourceAddress is UnicastAddress src)
                    _ = _conn.SendAsync(BuildRawAck(local, src, msg.SequenceNumber));

                if (msg.Content is DeviceDescriptorResponse)
                    ddTcs.TrySetResult(msg.Content);
                else if (msg.Content is MemoryResponse && !readDone)
                    readTcs.TrySetResult(msg.Content);
                else if (msg.Content is MemoryResponse && readDone)
                    writeTcs.TrySetResult(msg.Content);
            }

            _conn.OnReceivedMessage += OnMessage;
            try
            {
                // 1. T_Connect
                await _conn.SendAsync(new LDataBase(address, false, 0, new Connect()));
                await Task.Delay(150);

                // 2. DeviceDescriptor_Read (S=0)
                await _conn.SendAsync(new LDataBase(address, true, 0, new DeviceDescriptorRead()));
                await Task.WhenAny(ddTcs.Task, Task.Delay(3000));
                await Task.Delay(100);

                // 3. Memory_Read (S=1) — aktuellen Wert lesen
                await _conn.SendAsync(new LDataBase(address, true, 1, new MemoryRead(memAddress, 1)));
                if (await Task.WhenAny(readTcs.Task, Task.Delay(5000)) != readTcs.Task)
                    throw new TimeoutException($"Keine MemoryResponse (Read) von {address}");
                readDone = true;
                await Task.Delay(100);

                // 4. Memory_Write (S=2)
                await _conn.SendAsync(new LDataBase(address, true, 2, new MemoryWrite(memAddress, 1, new byte[] { value })));
                if (await Task.WhenAny(writeTcs.Task, Task.Delay(5000)) != writeTcs.Task)
                    throw new TimeoutException($"Keine MemoryResponse (Write) von {address}");
            }
            finally
            {
                await _conn.SendAsync(new LDataBase(address, false, 0, new Disconnect()));
                _conn.OnReceivedMessage -= OnMessage;
            }
        }

        private byte[] BuildRawAck(UnicastAddress src, UnicastAddress dst, byte seqNum)
        {
            byte[] srcBytes = src.GetBytes();
            byte[] dstBytes = dst.GetBytes();
            byte ctrl = (byte)(0xC2 | ((seqNum & 0x0F) << 2));
            return new byte[]
            {
        0x29, 0x00, 0xB0, 0x60,
        srcBytes[0], srcBytes[1],
        dstBytes[0], dstBytes[1],
        0x00, ctrl
            };
        }
    }
}