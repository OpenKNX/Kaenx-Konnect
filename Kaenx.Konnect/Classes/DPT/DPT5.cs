using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaenx.Konnect.Classes.DPT
{
    public class DPT5 : IDPT
    {
        public T GetValue<T>(byte[] value)
        {
            List<byte> values = new List<byte>(value.Reverse());
            if (values.Count == 1) values.Add(0x00);
            uint val2 = BitConverter.ToUInt16(values.ToArray(), 0); //Check reverse

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.String:
                    return (T)Convert.ChangeType(val2, typeof(T));

                case TypeCode.Object:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return (T)Convert.ChangeType(val2, typeof(T));
            }
            
            throw new Exception("DPT1 kann nicht in " + typeof(T).Name + " umgewandelt werden.");
        }

        public byte[] GetBytes(object value)
        {
            uint v;

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.String:
                    if(!uint.TryParse(value.ToString(), out v)) {
                        throw new Exception(value.ToString() +  " kann nicht in DPT5 (usigned int) umgewandelt werden.");
                    }
                    break;

                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    v = (uint)value;
                    break;

                default:
                    throw new Exception(value.GetType().ToString() +  " kann nicht in DPT5 (unsigned int) umgewandelt werden.");
            }
            
            if(v > 255)
                throw new Exception(v.ToString() +  " ist zu groß für DPT5 (unsigned int; 0-255)");

            return new byte[] { BitConverter.GetBytes(v)[0] };
        }
    }
}
