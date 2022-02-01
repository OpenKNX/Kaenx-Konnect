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
            uint val2 = BitConverter.ToUInt16(value.Reverse().ToArray(), 0); //Check reverse

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
    }
}
