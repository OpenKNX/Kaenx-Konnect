using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Interfaces
{
    public class KnxInterfaceIp : IKnxInterface
    {
        public string Name { get; set; }
        public int Port { get; set; }
        public string IP { get; set; }
        public DateTime LastFound { get; set; }

        public IPEndPoint Endpoint { get; set; }

        public string Hash
        {
            get
            {
                return Name + "#IP#" + Endpoint;
            }
        }

        public string Description
        {
            get
            {
                return Endpoint.Address + ":" + Endpoint.Port;
            }
        }
    }
}
