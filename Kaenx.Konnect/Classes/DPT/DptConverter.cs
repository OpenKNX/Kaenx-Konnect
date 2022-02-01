using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kaenx.Konnect.Classes.DPT
{
    public class DptConverter
    {
        public static T To<T>(DPTs dpt, byte[] value)
        {
            List<string> temp = new List<string>();
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();

            if (types.Any(t => t.IsClass && !t.IsNested && t.Name == dpt.ToString())) {
                Type type = types.Single(t => t.IsClass && !t.IsNested && t.Name == dpt.ToString());
                IDPT idpt = (IDPT)Activator.CreateInstance(type);
                return idpt.GetValue<T>(value);
            }

            throw new NotImplementedException(dpt.ToString() + " wurde noch nicht implementiert");
        }
    }
}
