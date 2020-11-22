using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Kaenx.Konnect.Messages.Request
{
    /// <summary>
    /// Creates a telegram to search for interfaces (only for IP and cEMI)
    /// </summary>
    public class MsgSearch : IMessageRequest
    {
        private IPEndPoint _endpoint;

        /// <summary>
        /// Creates a telegram to search for interfaces (only for IP and cEMI)
        /// </summary>
        public MsgSearch() { }

        public byte[] GetBytesCemi()
        {
            SearchRequest builder = new SearchRequest();
            builder.Build(_endpoint);
            return builder.GetBytes();
        }

        public byte[] GetBytesEmi1()
        {
            throw new NotImplementedException();
        }

        public byte[] GetBytesEmi2()
        {
            throw new NotImplementedException();
        }

        public void SetInfo(byte channel, byte seqCounter) { }

        public void SetSequenzeNumb(int seq) { }

        public void SetEndpoint(IPEndPoint endpoint)
        {
            _endpoint = endpoint;
        }
    }
}
