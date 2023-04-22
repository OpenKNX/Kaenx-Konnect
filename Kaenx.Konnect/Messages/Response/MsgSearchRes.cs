using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Parser;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Messages.Response
{
    public class MsgSearchRes : IMessageResponse
    {
        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = false;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress SourceAddress { get; set; }
        public IKnxAddress DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.PropertyValueWrite;
        public byte[] Raw { get; set; }



        public MsgSearchRes(byte[] data) => Raw = data;
        public MsgSearchRes() { }

        public IPEndPoint Endpoint { get; set; }
        public string FriendlyName { get; set; }
        public UnicastAddress PhAddr { get; set; }


        public void ParseDataCemi()
        {
            byte[] addr = new byte[4] { Raw[2], Raw[3], Raw[4], Raw[5] };
            Endpoint = new IPEndPoint(new IPAddress(addr), BitConverter.ToUInt16(new byte[2] { Raw[7], Raw[6] }, 0));
            byte[] phAddr = new byte[2] { Raw[12], Raw[13] };
            PhAddr = UnicastAddress.FromByteArray(phAddr);
            int total = Convert.ToInt32(Raw[8]);
            FriendlyName = System.Text.Encoding.UTF8.GetString(Raw, 32, total - 32).Trim();
        }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgSearchRes");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgSearchRes");
        }


        public byte[] GetBytesEmi1()
        {
            throw new NotImplementedException("GetBytesEmi1 - MsgSearchRes");
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException("GetBytesEmi2 - MsgSearchRes");
        }

        public byte[] GetBytesCemi()
        {
            List<byte> bytes = new List<byte>() { 0x06, 0x10, 0x02, 0x02, 0x00, 0x4C }; // Length, Version, Descriptor 2x, Total length 2x
            bytes.AddRange(new HostProtocolAddressInformation(0x01, Endpoint).GetBytes());

            //DIB DevInfo 
            bytes.Add((byte)(32 + FriendlyName.Length)); //Structure Length
            bytes.Add(0x01); //Description Type
            bytes.Add(0x02); //Medium TP
            bytes.Add(0x00); //Device Status ProgMode
            bytes.AddRange(new byte[] { 0x00, 0x00 }); //Project INstallation Identifier
            bytes.AddRange(new byte[] { 0xFF, 0xFF, 0x00, 0x01, 0x02, 0x03 }); //Knx SerialNumber
            bytes.AddRange(new byte[] { 0xe0, 0x00, 0x17, 0x0c }); //Multicast Address
            bytes.AddRange(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05 }); //MAC Address
            bytes.AddRange(Encoding.UTF8.GetBytes(FriendlyName));

            //DIB SuppSvec
            bytes.Add(0x08); //Structure Length
            bytes.Add(0x02); //Description Type
            bytes.Add(0x02); //Service Family: Core
            bytes.Add(0x01); //Service Version: v1
            bytes.Add(0x03); //ServiceFamily: Device Managment
            bytes.Add(0x01); //Service Version: v1
            bytes.Add(0x04); //Serice Family: Tunneling
            bytes.Add(0x01); //Service Version: v1

            return bytes.ToArray();
        }
    }
}
