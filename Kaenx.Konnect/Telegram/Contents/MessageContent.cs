using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.EMI.LData;
using Kaenx.Konnect.Enums;
using Kaenx.Konnect.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.Contents
{
    public class MessageContent : IContent
    {


        private LDataBase _message;
        private ExternalMessageInterfaces _emi;
        public int Length { get; private set; }

        public MessageContent(LDataBase message, ExternalMessageInterfaces emi)
        {
            _message = message;
            _emi = emi;
        }

        public MessageContent(byte[] data, ExternalMessageInterfaces emi)
        {
            _emi = emi;
            _message = new LDataBase(data, emi);
            Length = data.Length;
        }

        public byte[] ToByteArray()
        {
            List<byte> data = new();

            if (_message.SourceAddress is null)
                _message.SourceAddress = UnicastAddress.FromString("0.0.0");
            //throw new InvalidOperationException("Message SourceAddress cannot be null for cEMI");
            if (_message.DestinationAddress is null)
                throw new InvalidOperationException("Message SourceAddress cannot be null for cEMI");

            switch (_emi)
            {
                case ExternalMessageInterfaces.Emi1:
                    throw new NotImplementedException("EMI1 not implemented yet");

                case ExternalMessageInterfaces.Emi2:
                    throw new NotImplementedException("EMI2 not implemented yet");

                case ExternalMessageInterfaces.cEmi:
                    BitArray ctrlByte = new BitArray(new byte[] { 0xb0 });
                    BitArray drlByte = new BitArray(new byte[] { 0xe0 });

                    data.Add(0x00); // Control Byte will be set later
                    drlByte.Set(7, _message.DestinationAddress is MulticastAddress);
                    data.Add(bitToByte(drlByte)); // DRL Byte

                    data.AddRange(_message.SourceAddress.GetBytes());
                    data.AddRange(_message.DestinationAddress.GetBytes());

                    byte[] payload = _message.GetBytesCemi();
                    byte lengthData = (byte)payload.Length;

                    byte[] apci = BitConverter.GetBytes((short)_message.GetApciType()).Reverse().ToArray();
                    //if (withData.Contains(_message.GetApciType()) && lengthData != 0)
                    //{
                    //    lengthData--;
                    //    apci[1] = (byte)(payload[0] & 0x3F);
                    //    payload = payload.Skip(1).ToArray();
                    //}

                    if (_message.GetApciType() == ApciTypes.Connect ||
                        _message.GetApciType() == ApciTypes.Disconnect ||
                        _message.GetApciType() == ApciTypes.Ack ||
                        _message.GetApciType() == ApciTypes.NAK)
                    {
                        data.Add(0x00); // Length
                        byte tpci = 0x80; // TPCI Control Unnumbered
                        // For Connect and Disconnect we need to add the sequence number
                        tpci |= (byte)(apci[0] & 0x3);
                        data.Add(tpci);
                    } else
                    {
                        // TODO check if data is in tpci
                        lengthData++;
                        data.Add(lengthData);

                        byte tpci = 0x00; // TPCI Data
                        byte _sequenceNumber = (byte)_message.SequenceNumber;
                        if(_sequenceNumber != 255)
                        {
                            tpci |= 0x40; // Numbered
                            tpci |= (byte)((_sequenceNumber & 0x0F) << 2);
                        }
                        tpci |= (byte)(apci[0] & 0x3);
                        //tpci |= (byte)((_sequenceNumber & 0x0F) << 2); // Sequence Number
                        data.Add(tpci);
                        data.Add(apci[1]);
                    }

                    if (lengthData > 15) ctrlByte.Set(7, false);
                        data[0] = bitToByte(ctrlByte);



                    data.AddRange(payload);
                    break;
                default:
                    throw new NotImplementedException("Unknown ExternalMessageInterface: " + _emi.ToString());
            }

            Length = data.Count;
            return data.ToArray();
        }

        //private void SetPriority(Prios prio)
        //{
        //    switch (prio)
        //    {
        //        case Prios.System:
        //            ctrlByte.Set(2, true);
        //            ctrlByte.Set(3, true);
        //            break;

        //        case Prios.Alarm:
        //            ctrlByte.Set(2, true);
        //            ctrlByte.Set(3, false);
        //            break;

        //        case Prios.High:
        //            ctrlByte.Set(2, false);
        //            ctrlByte.Set(3, true);
        //            break;

        //        case Prios.Low:
        //            ctrlByte.Set(2, false);
        //            ctrlByte.Set(3, false);
        //            break;
        //    }
        //}

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
