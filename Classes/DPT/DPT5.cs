using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaenx.Konnect.Classes.DPT
{
    public class DPT5 : IDPT
    {
        /// <summary>
        /// DPT5 is an unsigned 8-bit integer (0-255). The value is stored in the second byte of the data array, while the first byte is reserved for the DPT type identifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T GetValue<T>(byte[] value)
        {
            if(value.Length > 2)
                throw new Exception("DPT5 value can not exceed 2 bytes.");

            byte val = value.Length == 2 ? value[1] : value[0];

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.String:
                    return (T)Convert.ChangeType(val, typeof(T));

                case TypeCode.Byte:
                    return (T)Convert.ChangeType(val, typeof(T));
    
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return (T)Convert.ChangeType(val, typeof(T));
            }
            
            throw new Exception("DPT5 kann nicht in " + typeof(T).Name + " umgewandelt werden.");
        }

        /// <summary>
        /// Will return the byte value for DPT5 (unsigned int; 0-255). The value will be stored in the second byte of the data array, while the first byte is reserved for the DPT type identifier.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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

                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    v = (uint)value;
                    break;

                default:
                    throw new Exception(value.GetType().ToString() +  " kann nicht in DPT5 (unsigned int) umgewandelt werden.");
            }
            
            if(v > 255)
                throw new Exception(v.ToString() +  " ist zu groß für DPT5 (unsigned int; 0-255)");

            return new byte[] { 0x00, BitConverter.GetBytes(v)[0] };
        }
    }
}
