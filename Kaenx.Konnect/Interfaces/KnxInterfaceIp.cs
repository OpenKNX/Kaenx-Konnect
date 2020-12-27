using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Interfaces
{
    [Serializable]
    public class KnxInterfaceIp : IKnxInterface
    {
        public string Name { get; set; }
        public int Port { get; set; }
        public string IP { get; set; }
        public string Auth { get; set; }
        public DateTime LastFound { get; set; }

        [field:NonSerialized]
        public IPEndPoint Endpoint
        {
            get { return new IPEndPoint(IPAddress.Parse(IP), Port); }
        }

        public bool IsRemote { get; set; } = false;

        public string Hash
        {
            get
            {
                return Name + "#IP#" + Endpoint + Auth + (IsRemote ? "Remote":"");
            }
        }

        public string Description
        {
            get
            {
                return Endpoint.Address + ":" + Endpoint.Port + (IsRemote ? " Remote" : "");
            }
        }
    }
}
