using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Messages.Response
{
    public class MsgPropertyReadRes : IMessageResponse
    {
        public byte ChannelId { get; set; }
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.PropertyValueResponse;
        public byte[] Raw { get; set; }



        public MsgPropertyReadRes(byte[] data) => Raw = data;
        public MsgPropertyReadRes() { }

        public int ObjectIndex { get; set; }
        public int PropertyId { get; set; }
        public int Length { get; set; }
        public byte[] Data { get; set; }



        public T Get<T>()
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.String:
                    string datas = BitConverter.ToString(Raw.Skip(4).ToArray()).Replace("-", "");
                    return (T)Convert.ChangeType(datas, typeof(T));

                case TypeCode.Int32:
                    byte[] datai = Raw.Skip(4).Reverse().ToArray();
                    byte[] xint = new byte[4];

                    for (int i = 0; i < datai.Length; i++)
                    {
                        xint[i] = datai[i];
                    }
                    return (T)Convert.ChangeType(BitConverter.ToInt32(xint, 0), typeof(T));

                case TypeCode.UInt32:
                    byte[] datai = Raw.Skip(4).Reverse().ToArray();
                    byte[] xint = new byte[4];

                    for (int i = 0; i < datai.Length; i++)
                    {
                        xint[i] = datai[i];
                    }
                    return (T)Convert.ChangeType(BitConverter.ToUInt32(xint, 0), typeof(T));

                default:
                    try
                    {
                        return (T)Convert.ChangeType(Raw.Skip(4).ToArray(), typeof(T));
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Data kann nicht in angegebenen Type konvertiert werden. " + typeof(T).ToString(), e);
                    }
            }
        }



        public void ParseDataCemi()
        {
            ObjectIndex = BitConverter.ToInt16(new byte[] { Raw[0], 0x00 }, 0);
            PropertyId = BitConverter.ToInt16(new byte[] { Raw[1], 0x00 }, 0);
            Data = Raw.Skip(4).ToArray();
        }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgPropertyReadRes");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgPropertyReadRes");
        }

        public byte[] GetBytesEmi1()
        {
            throw new NotImplementedException("GetBytesEmi1 - MsgPropertyReadRes");
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException("GetBytesEmi2 - MsgPropertyReadRes");
        }

        public byte[] GetBytesCemi()
        {
            throw new NotImplementedException("GetBytesCemi - MsgPropertyReadRes");
        }
    }
}
