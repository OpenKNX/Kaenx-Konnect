using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Kaenx.Konnect.Classes.Helper
{
    public class ResourcenHelper
    {
        public static DeviceResource GetDeviceResource(string resourceId, string maskId)
        {
            XDocument master = GetKnxMaster();
            if (master.Root == null)
                throw new Exception("Cant create Master");
            XElement mask = master.Descendants(XName.Get("MaskVersion", master.Root.Name.NamespaceName)).Single(mv => mv.Attribute("Id")?.Value == maskId);
            XElement? prop = null;
            try
            {
                prop = mask.Descendants(XName.Get("Resource", master.Root.Name.NamespaceName)).First(mv => mv.Attribute("Name")?.Value == resourceId);
                if (prop == null)
                    throw new Exception("Resource not found");
            }
            catch
            {
                throw new Exception("Device does not support this Property");
            }

            XElement? loc = prop.Element(XName.Get("Location", master.Root.Name.NamespaceName));
            if (loc == null)
                throw new Exception("Location not found");
            int length = int.Parse(prop.Element(XName.Get("ResourceType", master.Root.Name.NamespaceName))?.Attribute("Length")?.Value ?? "0");

            switch (loc.Attribute("AddressSpace")?.Value)
            {
                case "SystemProperty":
                {
                    DeviceResource device = new DeviceResource();
                    device.AddressSpace = AddressSpace.SystemProperty;
                    device.InterfaceObjectRef = uint.Parse(loc.Attribute("InterfaceObjectRef")?.Value ?? throw new Exception("InterfaceObjectRef not found"));
                    device.PropertyID = uint.Parse(loc.Attribute("PropertyID")?.Value ?? throw new Exception("PropertyID not found"));
                    device.Length = length;
                    return device;
                }

                case "StandardMemory":
                {
                    DeviceResource device = new DeviceResource();
                    device.AddressSpace = AddressSpace.StandardMemory;
                    device.Address = uint.Parse(loc.Attribute("StartAddress")?.Value ?? throw new Exception("StartAddress not found"));
                    device.Length = length;
                    return device;
                }

                case "Pointer":
                    string? newProp = loc.Attribute("PtrResource")?.Value;
                    if (newProp == null)
                        throw new Exception("Pointer Resource not found");
                    return GetDeviceResource(newProp, maskId);

                case "RelativeMemory":
                {
                    DeviceResource device = new DeviceResource();
                    device.AddressSpace = AddressSpace.SystemProperty;
                    device.InterfaceObjectRef = uint.Parse(loc.Attribute("InterfaceObjectRef")?.Value ?? throw new Exception("InterfaceObjectRef not found"));
                    device.PropertyID = uint.Parse(loc.Attribute("PropertyID")?.Value ?? throw new Exception("PropertyID not found"));
                    device.Length = length;
                    return device;
                }

                default:
                    throw new Exception("AddressSpace not found or unknown");
            }
        }

        public static XDocument GetKnxMaster()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("Kaenx.Konnect.Assets.knx_master.xml"))
            using (StreamReader reader = new StreamReader(stream))
            {
                string content = reader.ReadToEnd();
                return XDocument.Parse(content);
            }
        }
    }
}
