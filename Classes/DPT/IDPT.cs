using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Classes.DPT
{
    public interface IDPT
    {
        public T GetValue<T>(byte[] value);
        public byte[] GetBytes(object value);
    }
}
