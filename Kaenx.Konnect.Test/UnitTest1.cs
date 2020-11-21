using Kaenx.Konnect.Parser;
using Kaenx.Konnect.Responses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Kaenx.Konnect.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void ParseConnectResponse()
        {
            IQueryable<byte> response = new List<byte>
            {
                0x06,
                0x10,
                0x02,
                0x06,
                0x00,
                0x14,
                0x0d,
                0x00,
                0x08,
                0x01,
                0xc0,
                0xa8,
                0xb2,
                0xde,
                0x0e,
                0x57,
                0x04,
                0x04,
                0x24,
                0x00
            }.AsQueryable();

            ConnectResponse resp = new ConnectResponseParser().Build(6, 10, 20, response.Skip(6).ToArray());

            IPAddress addr = new IPAddress(new byte[] { 192, 168, 178, 222 });

            CollectionAssert.AreEqual(addr.GetAddressBytes(), resp.DataEndpoint.IpEndPoint.Address.GetAddressBytes());
        }
    }
}
