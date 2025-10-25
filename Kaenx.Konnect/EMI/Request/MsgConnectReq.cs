﻿using Kaenx.Konnect.Addresses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    /// <summary>
    /// Creates a telegram to connect to a device
    /// </summary>
 
    public class MsgConnectReq : IMessageRequest
    {
        public byte ChannelId { get; set; }
        public bool IsNumbered { get; } = true;
        public byte SequenceCounter { get; set; }
        public int SequenceNumber { get; set; }
        public IKnxAddress? SourceAddress { get; set; }
        public IKnxAddress? DestinationAddress { get; set; }
        public ApciTypes ApciType { get; } = ApciTypes.Connect;
        public byte[] Raw { get; set; } = new byte[0];



        /// <summary>
        /// Creates a telegram to connect to a device
        /// </summary>
        /// <param name="address">Unicast Address from device</param>
        public MsgConnectReq(UnicastAddress address)
        {
            DestinationAddress = address;
        }

        public MsgConnectReq() { }


        public byte[] GetBytesCemi()
        {
            return new byte[0];
        }

        public byte[] GetBytesEmi1()
        {
            //Emi2Request builder = new Emi2Request();
            //builder.Build(DestinationAddress, ApciTypes.Connect, 255);
            //return builder.GetBytes();
            throw new NotImplementedException("GetBytesEmi1 - MsgConnectReq");
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException("GetBytesEmi2 - MsgConnectReq");
        }



        public void ParseDataCemi()
        {
            
        }

        public void ParseDataEmi1()
        {
            
        }

        public void ParseDataEmi2()
        {
            
        }
    }
}
