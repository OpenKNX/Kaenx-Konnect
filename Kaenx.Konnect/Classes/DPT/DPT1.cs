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
            }
            
            throw new Exception("DPT1 kann nicht in " + typeof(T).Name + " umgewandelt werden.");
        }
    }
}
