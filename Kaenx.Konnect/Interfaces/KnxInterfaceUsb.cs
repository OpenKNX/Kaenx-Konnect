using Device.Net;
using Hid.Net.Windows;
using Usb.Net.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace Kaenx.Konnect.Interfaces
{
    [Serializable]
    public class KnxInterfaceUsb : IKnxInterface
    {
        public string Name { get; set; }
        public ConnectedDeviceDefinition ConnDefinition { get; set; }
        public string Serial { get; set; }
        public DateTime LastFound { get; set; }
        public bool IsRemote { get; } = false;


        public static KnxInterfaceUsb CheckHid(ConnectedDeviceDefinition def)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("knx_interfaces.xml"));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                XDocument doc = XDocument.Parse(result);
                var inters = doc.Descendants(XName.Get("Interface"));

                if (!inters.Any(inter => inter.Attribute("VendorID").Value == Convert.ToInt32(def.VendorId).ToString("X") && inter.Attribute("ProductID").Value == Convert.ToInt32(def.ProductId).ToString("X")))
                    return null;

                XElement xinter = inters.Single(inter => inter.Attribute("VendorID").Value == Convert.ToInt32(def.VendorId).ToString("X") && inter.Attribute("ProductID").Value == Convert.ToInt32(def.ProductId).ToString("X"));
                IEnumerable<XElement> trans = xinter.Descendants(XName.Get("Translation"));

                string name = xinter.Attribute("DefaultDisplayText").Value;
                string current = System.Globalization.CultureInfo.CurrentCulture.Name;

                if(trans.Any(t => t.Attribute("Language").Value == current))
                {
                    XElement translation = trans.Single(t => t.Attribute("Language").Value == current);
                    name = translation.Attribute("Text").Value;
                } else if(trans.Any(t => t.Attribute("Language").Value.StartsWith(current.Substring(0, 3)))){
                    XElement translation = trans.First(t => t.Attribute("Language").Value.StartsWith(current.Substring(0, 3)));
                    name = translation.Attribute("Text").Value;
                }


                return new KnxInterfaceUsb()
                {
                    Name = name,
                    ConnDefinition = def
                };
            }
        }


        public string Description
        {
            get
            {
                return "USB Gerät kommt hier ganz viel rein" + (IsRemote ? "Remote" : "");
            }
        }

        public string Hash
        {
            get
            {
                return Name + "#USB#" + Serial + (IsRemote ? " Remote" : "");
            }
        }
    }
}
