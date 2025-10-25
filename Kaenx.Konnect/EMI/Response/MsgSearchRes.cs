using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Messages.Response
{
    public class MsgSearchRes : IMessageResponse
    {
        public enum MediumTypes
        {
            TP0 = 0x00,
            TP1 = 0x01,
            PL110 = 0x02,
            PL132 = 0x03,
            RF = 0x04,
            IP = 0x05,
        }

        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = false;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress? SourceAddress { get; set; }
        public IKnxAddress? DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.Undefined;
        public byte[] Raw { get; set; } = new byte[0];



        public MsgSearchRes(byte[] data) => Raw = data;
        public MsgSearchRes() { }

        public byte MediumType { get; set; }
        public IPEndPoint? Endpoint { get; set; }
        public IPEndPoint? Multicast { get; set; }
        public string FriendlyName { get; set; } = "";
        public UnicastAddress? PhAddr { get; set; }
        public List<ServiceFamily> SupportedServiceFamilies = new List<ServiceFamily>();

        public void ParseDataCemi()
        {
            if (Raw.Length < 62)
                throw new Exception("Invalid raw length");

            //HPAI
            //0 = length (8)
            //1 = ipv4/ipv6
            //2-5 = ip
            byte[] addr = new byte[4] { Raw[2], Raw[3], Raw[4], Raw[5] };
            //6-7 = port
            Endpoint = new IPEndPoint(new IPAddress(addr), BitConverter.ToUInt16(new byte[2] { Raw[7], Raw[6] }, 0));
            
            //DIB DevInfo
            //0 =  length (54)
            //1 = DescriptionType
            MediumType = Raw[8+2];
            //3 = DeviceState (ProgMode)
            //4-5 = Physical Address
            byte[] phAddr = new byte[2] { Raw[8+4], Raw[8+5] };
            PhAddr = UnicastAddress.FromByteArray(phAddr);
            //6-7 = Project Installation Identifier
            //8-13 = SerialNumber
            //14-17 = MulticastAddress
            addr = new byte[4] { Raw[8+14], Raw[8+15], Raw[8+16], Raw[8+17] };
            Multicast = new IPEndPoint(new IPAddress(addr), 3671);
            //18-23 = MAC Address
            //24-53 = FriendlyName
            byte[] name = Raw.Skip(32).Take(30).ToArray();
            int nameLength = 0;
            for(int i = 0; i < name.Length; i++)
            {
                if(name[i] == 0)
                    break;
                nameLength++;
            }
            FriendlyName = System.Text.Encoding.UTF8.GetString(name, 0, nameLength);

            //DIB SupDvs
            //0 = length (different)
            //1 = DescriptionType
            int offset = 8+54;
            SupportedServiceFamilies.Clear();
            for(int i = 2; i <= Raw[54+8]-2; i+=2)
            {
                SupportedServiceFamilies.Add(new ServiceFamily() {
                    ServiceFamilyType = (ServiceFamilyTypes)Enum.ToObject(typeof(ServiceFamilyTypes), Raw[offset+i]),
                    Version = Raw[offset+i+1]
                });
            }
        }

        public bool IsMediumType(MediumTypes mediumType)
        {
            return (((int)MediumType >> (int)mediumType) & 0x1) != 0;
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
            if(Endpoint == null || Multicast == null || PhAddr == null)
                throw new Exception("Endpoint, Multicast or PhAddr is null");
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
