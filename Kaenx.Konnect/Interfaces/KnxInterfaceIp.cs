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
        public bool IsRemote { get; } = false;
        public Type InterfaceType
        {
            get
            {
                return this.GetType();
            }
        }

        [field:NonSerialized]
        public IPEndPoint Endpoint
        {
            get { return new IPEndPoint(IPAddress.Parse(IP), Port); }
        }

        public string Hash
        {
            get
            {
                return Name + "#IP#" + Endpoint + Auth;
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
