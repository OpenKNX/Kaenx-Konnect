﻿using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.EMI.DataMessages
{
    public class FunctionPropertyStateResponse : IDataMessage
    {
        public ApciTypes ApciType => StaticApciType;
        public static ApciTypes StaticApciType => ApciTypes.FunctionPropertyStateResponse;

        public int ObjectIndex { get; private set; }
        public int PropertyId { get; private set; }
        public ReturnCodes ReturnCode { get; private set; }
        public byte[] Data { get; private set; }

        public FunctionPropertyStateResponse(int objectIndex, int propertyId, ReturnCodes returnCode, byte[] data)
        {
            ObjectIndex = objectIndex;
            PropertyId = propertyId;
            ReturnCode = returnCode;
            Data = data;
        }

        public FunctionPropertyStateResponse(byte[] data, ExternalMessageInterfaces emi)
        {
            switch (emi)
            {
                case ExternalMessageInterfaces.cEmi:
                    ParseDataCemi(data);
                    break;
                case ExternalMessageInterfaces.Emi1:
                    ParseDataEmi1(data);
                    break;
                case ExternalMessageInterfaces.Emi2:
                    ParseDataEmi2(data);
                    break;
                default:
                    throw new NotImplementedException("Unknown ExternalMessageInterface: " + emi.ToString());
            }
        }

        public byte[] GetBytesCemi()
        {
            List<byte> data = new List<byte>();
            data.Add((byte)(ObjectIndex & 0xFF));
            data.Add((byte)(PropertyId & 0xFF));
            data.Add((byte)ReturnCode);
            data.AddRange(Data);
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
            ObjectIndex = data[0];
            PropertyId = data[1];
            ReturnCode = (ReturnCodes)data[2];
            Data = data.Skip(3).ToArray();
        }

        public void ParseDataEmi1(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void ParseDataEmi2(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
