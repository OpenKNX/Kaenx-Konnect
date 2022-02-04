using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kaenx.Konnect.Classes.DPT
{
    public class DptConverter
    {
        public static T FromByteArray<T>(DPTs dpt, byte[] value)
        {
            IDPT idpt = GetDptClass(dpt.ToString());
            return idpt.GetValue<T>(value);
        }

        public static byte[] ToByteArray(DPTs dpt, object value)
        {
            IDPT idpt = GetDptClass(dpt.ToString());
            return idpt.GetBytes(value);
        }

        private static IDPT GetDptClass(string name) {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();

            if (types.Any(t => t.IsClass && !t.IsNested && t.Name == name)) {
                Type type = types.Single(t => t.IsClass && !t.IsNested && t.Name == name);
                return (IDPT)Activator.CreateInstance(type);
            }
            throw new NotImplementedException(name + " wurde noch nicht implementiert");
        }
    }
}
