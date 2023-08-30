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
    class RoutingParser : IReceiveParser
    {
        public ushort ServiceTypeIdentifier => 0x0530;

        public RoutingParser() { }

        IParserMessage IReceiveParser.Build(byte headerLength, byte protocolVersion, ushort totalLength,
          byte[] responseBytes)
        {
            return Build(headerLength, protocolVersion, totalLength, responseBytes);
        }


        public Responses.RoutingResponse Build(byte headerLength, byte protocolVersion, ushort totalLength, byte[] responseBytes)
        {

            var messageCode = responseBytes[0];
            var addInformationLength = responseBytes[1];
            var controlField = responseBytes[2];
            var controlField2 = responseBytes[3];
            var npduLength = responseBytes[8];
            byte[] npdu;

            if(npduLength != 0)
            {
                npdu = new byte[] { responseBytes[9], responseBytes[10] };
            } else
            {
                npdu = new byte[] { responseBytes[9] };
            }


            byte[] data = null;
            int seqNumb = 0x0;
            ApciTypes type = ApciTypes.Undefined;

            bool isNumbered;
            if (npduLength != 0)
            {

                BitArray bitsNpdu = new BitArray(npdu);
                isNumbered = bitsNpdu.Get(6);
                if (isNumbered)
                {
                    seqNumb = npdu[0] >> 2;
                    seqNumb &= 0xF;
                }

                int apci1 = ((npdu[0] & 3) << 2) | (npdu[1] >> 6);

                switch (apci1)
                {
                    case 0:
                        type = ApciTypes.GroupValueRead;
                        break;
                    case 1:
                        type = ApciTypes.GroupValueResponse;
                        int datai2 = npdu[1] & 63;
                        //TODO check if this works? i guess not
                        data = new byte[responseBytes.Length - 8 + 1];
                        data[0] = Convert.ToByte(datai2);
                        for (int i = 1; i < responseBytes.Length - 8 + 1; i++)
                        {
                            data[i] = responseBytes[i];
                        }
                        break;
                    case 2:
                        type = ApciTypes.GroupValueWrite;
                        int datai = npdu[1] & 63;
                        data = new byte[responseBytes[8]];
                        data[0] = Convert.ToByte(datai);
                        for (int i = 11; i < responseBytes.Length; i++)
                        {
                            data[i - 11] = responseBytes[i];
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
                        if (npdu[1] == 0)
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

                if (data == null)
                {
                    if(responseBytes.Length > 11)
                    {
                        data = new byte[responseBytes.Length - 11];

                        int c = 0;
                        for (int i = 11; i < responseBytes.Length; i++)
                        {
                            data[c] = responseBytes[i];
                            c++;
                        }
                    } else {
                        data = new byte[0];
                    }
                }
            }
            else
            {
                data = new byte[0];
                int apci3 = npdu[0] & 3;

                BitArray bitsNpdu = new BitArray(npdu);
                isNumbered = bitsNpdu.Get(6);
                if (isNumbered)
                {
                    seqNumb = npdu[0] >> 2;
                    seqNumb &= 0xF;
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

            BitArray bitsCtrl1 = new BitArray(new[] { responseBytes[2] });
            BitArray bitsCtrl2 = new BitArray(new[] { responseBytes[3] });

            IKnxAddress destAddr;
            if (bitsCtrl2.Get(7))
                destAddr = MulticastAddress.FromByteArray(new[] { responseBytes[6], responseBytes[7] });
            else
                destAddr = UnicastAddress.FromByteArray(new[] { responseBytes[6], responseBytes[7] });

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

            return new RoutingResponse(headerLength,
                protocolVersion,
                totalLength,
                messageCode,
                addInformationLength,
                isNumbered,
                ackWanted,
                controlField,
                controlField2,
                UnicastAddress.FromByteArray(new[] { responseBytes[4], responseBytes[5] }),
                destAddr,
                type,
                seqNumb,
                data);
        }
    }
}
