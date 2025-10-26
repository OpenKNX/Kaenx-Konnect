using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.EMI.DataMessages;
using Kaenx.Konnect.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.EMI.LData
{
    public class LDataBase : IEmiMessage
    {
        private static List<ApciTypes> withData = new List<ApciTypes>() {
                ApciTypes.GroupValueRead, ApciTypes.GroupValueResponse, ApciTypes.GroupValueWrite,
                ApciTypes.ADCRead, ApciTypes.ADCResponse,
                ApciTypes.MemoryRead, ApciTypes.MemoryResponse, ApciTypes.MemoryWrite,
                ApciTypes.DeviceDescriptorRead, ApciTypes.DeviceDescriptorResponse, ApciTypes.Restart
            };

        // These have x bytes, but length must be increased by 1 for legacy reasons
        private static List<ApciTypes> withDataLegacy = new List<ApciTypes>() {
                ApciTypes.PropertyDescriptionResponse,
            };

        public static List<MessageCodes> SupportedMessageCodes { get; } = new List<MessageCodes>()
        {
            MessageCodes.L_Data_req,
            MessageCodes.L_Data_ind,
            MessageCodes.L_Data_con
        };

        public MessageCodes MessageCode { get; private set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; private set; }
        public bool IsControl { get; private set; }
        public bool IsNumbered { get; private set; }
        public byte SequenceNumber { get; private set; }
        public IDataMessage? Content { get; private set; }

        public byte[] AdditionalData { get; private set; } = Array.Empty<byte>();
        

        public LDataBase(byte[] data, ExternalMessageInterfaces emi)
        {
            switch (emi)
            {
                case ExternalMessageInterfaces.Emi1:
                    ParseDataEmi1(data);
                    break;
                case ExternalMessageInterfaces.Emi2:
                    ParseDataEmi2(data);
                    break;
                case ExternalMessageInterfaces.cEmi:
                    ParseDataCemi(data);
                    break;
                default:
                    throw new NotImplementedException("Unknown ExternalMessageInterface: " + emi.ToString());
            }
        }

        public LDataBase(UnicastAddress destination, bool isNumbered, byte sequenceNumber, IDataMessage content, MessageCodes messageCode = MessageCodes.L_Data_req)
        {
            MessageCode = messageCode;
            SourceAddress = UnicastAddress.FromString("0.0.0");
            DestinationAddress = destination;
            Content = content;
            IsNumbered = isNumbered;
            SequenceNumber = sequenceNumber;

            IsControl = (content.ApciType == ApciTypes.Connect ||
                         content.ApciType == ApciTypes.Disconnect ||
                         content.ApciType == ApciTypes.Ack ||
                         content.ApciType == ApciTypes.NAK);
        }

        public ApciTypes GetApciType()
        {
            if(Content != null)
            {
                return Content.ApciType;
            }
            return ApciTypes.Undefined;
        }

        public T? GetContent<T>()
        {
            if (Content is T typedContent)
            {
                return typedContent;
            }
            else
            {
                return (T?)Convert.ChangeType(null, typeof(T?));
            }
        }


        public byte[] GetBytesCemi()
        {
            List<byte> data = new List<byte>();
            data.Add((byte)MessageCode);
            data.Add((byte)AdditionalData.Length);
            data.AddRange(AdditionalData);

            BitArray ctrl1Byte = new BitArray(new byte[] { 0xb0 });
            BitArray ctrl2Byte = new BitArray(new byte[] { 0xe0 });

            data.Add(0x00); // Control Byte will be set later
            ctrl2Byte.Set(7, DestinationAddress is MulticastAddress);
            data.Add(bitToByte(ctrl2Byte)); // DRL Byte
            data.AddRange(SourceAddress.GetBytes());
            data.AddRange(DestinationAddress.GetBytes());


            byte[] payload = Content?.GetBytesCemi() ?? Array.Empty<byte>();
            byte lengthData = (byte)payload.Length;

            byte[] apci = BitConverter.GetBytes((short)GetApciType()).Reverse().ToArray();
            if (withData.Contains(GetApciType()) && lengthData != 0)
            {
                lengthData--;
                apci[1] = (byte)(payload[0] & 0x3F);
                payload = payload.Skip(1).ToArray();
            }
            if(withDataLegacy.Contains(GetApciType()))
            {
                lengthData++;
            }

            if (IsControl)
            {
                data.Add(0x00); // Length
                byte tpci = 0x80; // TPCI Control Unnumbered
                                  // For Connect and Disconnect we need to add the sequence number
                tpci |= (byte)(apci[0] & 0x3);
                if (IsNumbered)
                {
                    tpci |= 0x40; // Numbered
                    tpci |= (byte)((SequenceNumber & 0x0F) << 2);
                }
                data.Add(tpci);
            }
            else
            {
                // TODO check if data is in tpci
                lengthData++;
                data.Add(lengthData);

                byte tpci = 0x00; // TPCI Data
                if (IsNumbered)
                {
                    tpci |= 0x40; // Numbered
                    tpci |= (byte)((SequenceNumber & 0x0F) << 2);
                }
                tpci |= (byte)(apci[0] & 0x3);
                data.Add(tpci);
                data.Add(apci[1]);
            }

            if (lengthData > 15) ctrl1Byte.Set(7, false);
            data[2 + AdditionalData.Length] = bitToByte(ctrl1Byte);

            if (lengthData > 0)
                data.AddRange(payload);

            return data.ToArray();
        }

        public byte[] GetBytesEmi1()
        {
            throw new NotImplementedException();
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException();
        }

        public void ParseDataCemi(byte[] data)
        {
            IEnumerable<byte> dataEnum = data;
            MessageCode = (MessageCodes)data[0];
            int additionalDataLength = data[1];
            AdditionalData = data.Skip(2).Take(additionalDataLength).ToArray();
            dataEnum = dataEnum.Skip(2 + additionalDataLength);

            BitArray ctrl1Byte = new BitArray(dataEnum.Take(1).ToArray());
            BitArray ctrl2Byte = new BitArray(dataEnum.Skip(1).First());

            SourceAddress = UnicastAddress.FromByteArray(dataEnum.Skip(2).Take(2).ToArray());
            DestinationAddress = ctrl2Byte.Get(7) ?
                (IKnxAddress)MulticastAddress.FromByteArray(dataEnum.Skip(4).Take(2).ToArray()) :
                (IKnxAddress)UnicastAddress.FromByteArray(dataEnum.Skip(4).Take(2).ToArray());

            byte length = dataEnum.Skip(6).First();
            byte tpci = dataEnum.Skip(7).First();
            IsControl = (tpci & 0x80) == 0x80;
            IsNumbered = (tpci & 0x40) == 0x40;

            if(IsNumbered)
            {
                SequenceNumber = (byte)((tpci >> 2) & 0x0F);
            } else {
                SequenceNumber = 255;
            }

            ApciTypes apciType = ApciTypes.Undefined;
            if (IsControl)
            {
                int apci = ((tpci & 0x03) | 0x80) << 8;
                apciType = (ApciTypes)(apci);
            } else
            {
                byte apci0 = (byte)(tpci & 0x03);
                byte apci1 = dataEnum.Skip(8).First();

                int apci2 = (apci0 << 8) | (apci1 & 0xC0);
                ApciTypes apci3 = (ApciTypes)apci2;
                if(withData.Contains(apci3))
                {
                    apciType = apci3;
                } else
                {
                    apci2 = (apci0 << 8) | (apci1);
                    apciType = (ApciTypes)apci2;
                }
            }

            var q = from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.IsClass && t.IsNested == false && (t.Namespace == "Kaenx.Konnect.EMI.DataMessages")
                    select t;

            Debug.WriteLine($"Got APCI: {apciType.ToString()}");

            Type? messageType = null;
            foreach(Type t in q.ToList())
            {
                PropertyInfo? prop = t.GetProperty("StaticApciType");
                if(prop == null) continue;
                ApciTypes? apci1 = prop.GetValue(null) as ApciTypes?;
                if(apci1 == apciType)
                {
                    apciType = apci1 ?? ApciTypes.Undefined;
                    messageType = t;
                    break;
                }
            }

            if(messageType == null)
                throw new NotImplementedException("No IDataMessage found for ApciType: " + apciType.ToString());

            List<byte> payload = new List<byte>();

            if(withData.Contains(apciType))
            {
                byte firstDataByte = dataEnum.Skip(8).First();
                payload.Add((byte)(firstDataByte & 0x3F));
                payload.AddRange(dataEnum.Skip(9).Take(length - 1));
            }
            else if(withDataLegacy.Contains(apciType))
            {
                payload.AddRange(dataEnum.Skip(9).Take(length - 1));
            }
            else
            {
                payload.AddRange(dataEnum.Skip(8).Take(length));
            }

            object? message = Activator.CreateInstance(messageType, new object[] { payload.ToArray(), ExternalMessageInterfaces.cEmi });
            if(message == null)
                throw new NotImplementedException("Could not create instance of IDataMessage for ApciType: " + apciType.ToString());

            Content = (IDataMessage)message;
        }

        public void ParseDataEmi1(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void ParseDataEmi2(byte[] data)
        {
            throw new NotImplementedException();
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
}
