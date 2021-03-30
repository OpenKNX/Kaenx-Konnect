using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Messages;
using Kaenx.Konnect.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Builders
{
    class TunnelRequest : IRequestBuilder
    {
        private List<byte> bytes = new List<byte>();
        private bool IsExtended = false;

        private BitArray ctrlByte = new BitArray(new byte[] { 0xb0 });
        private BitArray drlByte = new BitArray(new byte[] { 0xe0 });

            //TODO sequenz obsolet machen!!
         public void Build(IKnxAddress sourceAddress, IKnxAddress destinationAddress, ApciTypes apciType, int sCounter = 255, byte[] data = null)
        {
            //bytes.Add(0x11); //Message Code
            //bytes.Add(0x00); //Manufacturer Code
            if(IsExtended) ctrlByte.Set(7, false);
            bytes.Add(bitToByte(ctrlByte)); // Control Byte



            drlByte.Set(7, destinationAddress is MulticastAddress);

            bytes.Add(bitToByte(drlByte)); // DRL Byte

            bytes.AddRange(sourceAddress.GetBytes()); // Source Address
            bytes.AddRange(destinationAddress.GetBytes()); // Destination Address

            byte lengthData = Convert.ToByte(data?.Length ?? 0);




            List<ApciTypes> datatypes = new List<ApciTypes>() { ApciTypes.Restart, ApciTypes.IndividualAddressRead, ApciTypes.DeviceDescriptorRead, ApciTypes.GroupValueRead, ApciTypes.GroupValueResponse, ApciTypes.GroupValueWrite, ApciTypes.ADCRead, ApciTypes.ADCResponse, ApciTypes.MemoryRead, ApciTypes.MemoryResponse, ApciTypes.MemoryWrite };

            int _apci = (int)apciType;
            if (apciType == ApciTypes.Ack)
                _apci--;
            _apci = _apci | ((sCounter == 255 ? 0 : sCounter) << 10);
            _apci = _apci | ((sCounter == 255 ? 0 : 1) << 14);
            _apci = _apci | (((data == null && !datatypes.Contains(apciType)) ? 1 : 0) << 15);


            List<ApciTypes> withData = new List<ApciTypes>() {
                ApciTypes.GroupValueRead, ApciTypes.GroupValueResponse, ApciTypes.GroupValueWrite,
                ApciTypes.IndividualAddressWrite, ApciTypes.IndividualAddressRead, ApciTypes.IndividualAddressResponse,
                ApciTypes.ADCRead, ApciTypes.ADCResponse,
                ApciTypes.MemoryRead, ApciTypes.MemoryResponse, ApciTypes.MemoryWrite,
                ApciTypes.DeviceDescriptorRead, ApciTypes.DeviceDescriptorResponse, ApciTypes.Restart
            };

            if (withData.Contains(apciType))
            {
                if (data != null)
                {
                    byte first = data[0];
                    if(first < 64)
                    {
                        first &= 0b00111111;
                        _apci |= first;
                        data = data.Skip(1).ToArray();
                        lengthData--;
                    }
                }
            }


            byte[] _apci2 = BitConverter.GetBytes(Convert.ToUInt16(_apci));

            switch(apciType)
            {
                case ApciTypes.ADCRead:
                case ApciTypes.ADCResponse:
                case ApciTypes.Ack:
                case ApciTypes.Connect:
                case ApciTypes.Disconnect:
                    bytes.Add(lengthData);
                    bytes.Add(_apci2[1]);
                    break;
                default:
                    bytes.Add(++lengthData);
                    bytes.Add(_apci2[1]);
                    bytes.Add(_apci2[0]);
                    break;
            }

            if(data != null)
                bytes.AddRange(data);
        }

        public byte[] GetBytes()
        {
            return bytes.ToArray();
        }

        public void SetIsExtended() {
            IsExtended = true;
        }



        public void SetPriority(Prios prio)
        {
            switch (prio)
            {
                case Prios.System:
                    ctrlByte.Set(2, true);
                    ctrlByte.Set(3, true);
                    break;

                case Prios.Alarm:
                    ctrlByte.Set(2, true);
                    ctrlByte.Set(3, false);
                    break;

                case Prios.High:
                    ctrlByte.Set(2, false);
                    ctrlByte.Set(3, true);
                    break;

                case Prios.Low:
                    ctrlByte.Set(2, false);
                    ctrlByte.Set(3, false);
                    break;

            }
        }


        private byte bitToByte(BitArray arr)
        {
            byte byteOut = 0;
            for (byte i = 0; i < arr.Count; i++)
            {
                if (arr[i])
                    byteOut |= (byte)(1 << i);
            }
            return byteOut;
        }
    }

    enum Prios
    {
        System,
        Alarm,
        High,
        Low
    }

}
