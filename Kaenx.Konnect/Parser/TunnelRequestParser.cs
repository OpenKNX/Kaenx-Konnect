using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Messages;
using Kaenx.Konnect.Responses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Kaenx.Konnect.Parser
{
    class TunnelRequestParser : IReceiveParser
    {
        public ushort ServiceTypeIdentifier => 0x0420;

        public TunnelRequestParser() { }

        IParserMessage IReceiveParser.Build(byte headerLength, byte protocolVersion, ushort totalLength,
          byte[] responseBytes)
        {
            return Build(headerLength, protocolVersion, totalLength, responseBytes);
        }


        public Requests.TunnelRequest Build(byte headerLength, byte protocolVersion, ushort totalLength, byte[] responseBytes)
        {
            var structureLength = responseBytes[0];
            var communicationChannel = responseBytes[1];
            var sequenceCounter = responseBytes[2];
            var messageCode = responseBytes[4];
            var addInformationLength = responseBytes[5];
            var controlField = responseBytes[6];
            var controlField2 = responseBytes[7];
            var npduLength = responseBytes[12];
            byte[] npdu;

            if(npduLength != 0)
            {
                npdu = new byte[] { responseBytes[13], responseBytes[14] };
            } else
            {
                npdu = new byte[] { responseBytes[13] };
            }


            byte[] data = null;
            int seqNumb = 0x0;
            bool isNumbered = false;
            ApciTypes type = ApciTypes.Undefined;

            if (npduLength != 0)
            {

                BitArray bitsNpdu = new BitArray(npdu);
                isNumbered = bitsNpdu.Get(6);
                if(isNumbered)
                {
                    seqNumb = npdu[0] >> 2;
                    seqNumb = seqNumb & 0xF;
                }

                int apci1 = ((npdu[0] & 3) << 2) | (npdu[1] >> 6 );

                switch(apci1)
                {
                    case 0:
                        type = ApciTypes.GroupValueRead;
                        break;
                    case 1:
                        type = ApciTypes.GroupValueResponse;
                        int datai2 = npdu[1] & 63;
                        data = new byte[responseBytes.Length - 15 + 1];
                        data[0] = Convert.ToByte(datai2);
                        for (int i = 1; i < responseBytes.Length - 15 + 1; i++)
                        {
                            data[i] = responseBytes[i];
                        }
                        break;
                    case 2:
                        type = ApciTypes.GroupValueWrite;
                        int datai = npdu[1] & 63;
                        data = new byte[responseBytes[12]];
                        data[0] = Convert.ToByte(datai);
                        for(int i = 15; i< responseBytes.Length; i++)
                        {
                            data[i-14] = responseBytes[i];
                        }
                        break;
                    case 3:
                        type = ApciTypes.IndividualAddressWrite;
                        break;
                    case 4:
                        type = ApciTypes.IndividualAddressRead;
                        break;
                    case 5:
                        type = ApciTypes.IndividualAddressResponse;
                        break;
                    case 6:
                        type = ApciTypes.ADCRead;
                        break;
                    case 7:
                        if(npdu[1] == 0) 
                            type = ApciTypes.ADCResponse;
                        break;
                    case 8:
                        type = ApciTypes.MemoryRead;
                        break;
                    case 9:
                        type = ApciTypes.MemoryResponse;
                        break;
                    case 10:
                        type = ApciTypes.MemoryWrite;
                        break;


                    default:
                        apci1 = ((npdu[0] & 3) << 8) | npdu[1];
                        type = (ApciTypes)apci1;
                        break;
                }

                if(data == null)
                {
                    data = new byte[responseBytes.Length - 15];

                    int c = 0;
                    for (int i = 15; i < responseBytes.Length; i++)
                    {
                        data[c] = responseBytes[i];
                        c++;
                    }
                }
            } else
            {
                data = new byte[0];
                int apci3 = npdu[0] & 3;

                BitArray bitsNpdu = new BitArray(npdu);
                isNumbered = bitsNpdu.Get(6);
                if (isNumbered)
                {
                    seqNumb = npdu[0] >> 2;
                    seqNumb = seqNumb & 0xF;
                }

                switch (apci3)
                {
                    case 0:
                        type = ApciTypes.Connect;
                        break;
                    case 1:
                        type = ApciTypes.Disconnect;
                        break;
                    case 2:
                        type = ApciTypes.Ack;
                        break;
                    case 3:
                        type = ApciTypes.NAK;
                        break;
                    default:
                        Debug.WriteLine("Unbekantes NPDU: " + apci3);
                        break;
                }
            }

            BitArray bitsCtrl1 = new BitArray(new[] { responseBytes[6] });
            BitArray bitsCtrl2 = new BitArray(new[] { responseBytes[7] });

            IKnxAddress destAddr = null;
            if (bitsCtrl2.Get(7))
                destAddr = MulticastAddress.FromByteArray(new[] { responseBytes[10], responseBytes[11] });
            else
                destAddr = UnicastAddress.FromByteArray(new[] { responseBytes[10], responseBytes[11] });

            bool ackWanted = bitsCtrl1.Get(2);

            switch (type)
            {
                case ApciTypes.MemoryWrite:
                case ApciTypes.MemoryRead:
                    byte[] data_temp = new byte[data.Length + 1];

                    int restData = npdu[1] & 127;

                    data_temp[0] = BitConverter.GetBytes(restData)[0];

                    for (int i = 0; i < data.Length; i++)
                    {
                        data_temp[i + 1] = data[i];
                    }
                    data = data_temp;
                    break;
            }

            return new Requests.TunnelRequest(headerLength, protocolVersion, totalLength, structureLength, communicationChannel,
              sequenceCounter, messageCode, addInformationLength, isNumbered, ackWanted, controlField, controlField2,
              UnicastAddress.FromByteArray(new[] { responseBytes[8], responseBytes[9] }),
              destAddr, type, seqNumb,
              data);
        }
    }
}
