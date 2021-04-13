using Kaenx.Konnect.Connections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Kaenx.Konnect.Interfaces
{
    [Serializable]
    public class KnxInterfaceRemote : IKnxInterface
    {
        public string Name { get; set; }
        public string DeviceId { get; set; }
        public string Serial { get; set; }
        public DateTime LastFound { get; set; }
        public bool IsRemote { get; } = true;
        public string RemoteHash { get; }
        public Type InterfaceType
        {
            get
            {
                return this.GetType();
            }
        }



        public string Description { get; }
        public string Hash { get; }


        public KnxInterfaceRemote(string description, string hash)
        {
            Description = description + " Remote";
            Hash = hash + "Remote";
            RemoteHash = hash;
        }
    }
}
