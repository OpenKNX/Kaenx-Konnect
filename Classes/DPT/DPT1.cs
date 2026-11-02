using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Classes.DPT
{
    public class DPT1: IDPT
    {
        public T GetValue<T>(byte[] value)
        {
            int val = value[0] & 0x01;

            switch(Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.String:
                    return (T)Convert.ChangeType(val == 1 ? "True" : "False", typeof(T));

                case TypeCode.Object:
                case TypeCode.Boolean:
                    return (T)Convert.ChangeType(val == 1, typeof(T));
                    
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    return (T)Convert.ChangeType(val, typeof(T));
            }
            
            throw new Exception("DPT1 kann nicht in " + typeof(T).Name + " umgewandelt werden.");
        }

        public byte[] GetBytes(object value)
        {
            byte output;

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.String:
                    string v = value.ToString()?.ToLower() ?? "";
                    output = (byte)((v == "on" || v == "true") ? 0x01 : 0x00);
                    break;

                case TypeCode.Boolean:
                    output = (byte)(((bool)value) ? 0x01 : 0x00);
                    break;

                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    uint v2 = Convert.ToUInt32(value);
                    output = (byte)((v2 != 0) ? 0x01 : 0x00);
                    break;

                default:
                    throw new Exception(value.GetType().ToString() +  " kann nicht in DPT1 (boolsch) umgewandelt werden.");
            }
            
            return new byte[] { output };
        }
    }
}
