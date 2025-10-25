using Kaenx.Konnect.Addresses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    public enum MsgSystemNetworkParameterReadOperand
    {
        ByProgrammingMode = 0x01,
        ByExFactoryState = 0x02,
        ByPowerReset = 0x03,
        ByManufacturerSpecific = 0xFE
    }

    /// <summary>
    /// Creates a telegram to read the device descriptor
    /// </summary>
    public class MsgSystemNetworkParameterReadReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = true;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress? SourceAddress { get; set; }
        public IKnxAddress? DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.DeviceDescriptorRead;
        public byte[] Raw { get; set; } = new byte[0];
        public MsgSystemNetworkParameterReadOperand Operand { get; set; }
        private byte[] _data = new byte[0];

        /// <summary>
        /// Creates a telegram to read the device descriptor
        /// </summary>
        /// <param name="address">Unicast Address from device</param>
        public MsgSystemNetworkParameterReadReq(MsgSystemNetworkParameterReadOperand operand, byte[] test_info)
        {
            DestinationAddress = MulticastAddress.FromString("0/0/0");
            Operand = operand;
            _data = test_info;
        }

        public MsgSystemNetworkParameterReadReq() { }


        public byte[] GetBytesCemi()
        {
            List<byte> data = new List<byte>() { 0x11, 0x00 };
            //TunnelRequest builder = new TunnelRequest();

            List<byte> payload = new()
            {
                0x00,
                0x00,
                0x00,
                0b10110000,
                (byte)Operand
            };

            if(_data != null)
                payload.AddRange(_data);

            //builder.Build(SourceAddress, DestinationAddress, ApciTypes.SystemNetworkParameterRead, 255, payload.ToArray());
            //data.AddRange(builder.GetBytes());
            return data.ToArray();
        }

        public byte[] GetBytesEmi1()
        {
            throw new NotImplementedException("GetBytesEmi2 - MsgReadSerialNumberReq");
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException("GetBytesEmi2 - MsgReadSerialNumberReq");
        }




        public void ParseDataCemi() {
            //No Data to parse
        }

        public void ParseDataEmi1()
        {
            throw new NotImplementedException("ParseDataEmi1 - MsgReadSerialNumberReq");
        }

        public void ParseDataEmi2()
        {
            throw new NotImplementedException("ParseDataEmi2 - MsgReadSerialNumberReq");
        }
    }
}
