using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Addresses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Responses;
using Kaenx.Konnect.Parser;
using System.Net.NetworkInformation;
using System.Linq;
using System.Diagnostics;
using Kaenx.Konnect.Connections;

namespace Kaenx.Konnect
{
    public class Connection
    {
        public IKnxConnection BusConnection { get; private set; }



        public Connection(IKnxConnection _conn)
        {
            BusConnection = _conn;
        }

        /// <summary>
        /// Send the Connect Telegram to the interface.
        /// </summary>
        public void Connect()
        {
            BusConnection.Connect();
        }

        /// <summary>
        /// Send the Disconnect Telegram to the interface.
        /// </summary>
        public void Disconnect()
        {
            BusConnection.Disconnect();
        }

        /// <summary>
        /// Send Status Request Telegram to the interface.
        /// </summary>
        public void SendStatusReq()
        {
            BusConnection.SendStatusReq();
        }


        /// <summary>
        /// Sends given Data to the Bus.
        /// </summary>
        /// <param name="builder">Telegram Builder</param>
        /// <returns>Returns Sequenz Counter</returns>
        public byte Send(IRequestBuilder builder)
        {
            return BusConnection.Send(builder);
        }

        /// <summary>
        /// Sends given Data to the bus, also when it is not connected.
        /// </summary>
        /// <param name="builder">Telegram Builder</param>
        public void SendWithoutConnected(IRequestBuilder builder)
        {
            byte[] data = builder.GetBytes();
            BusConnection.Send(data);
        }

        /// <summary>
        /// Sends given Data to the Bus asynchron.
        /// </summary>
        /// <param name="builder">Telegram Builder</param>
        /// <returns>Returns Sequenz Counter</returns>
        public async Task<byte> SendAsync(IRequestBuilder builder)
        {
            return BusConnection.Send(builder);
        }

        /// <summary>
        /// Sends given Data to the Bus asynchron.
        /// </summary>
        /// <param name="bytes">Telegram as bytes</param>
        /// <returns></returns>
        public Task SendAsync(byte[] bytes)
        {
            if (!BusConnection.IsConnected)
                throw new Exception("Roflkopter");

            BusConnection.Send(bytes);


            return Task.CompletedTask;
        }
    }
}
