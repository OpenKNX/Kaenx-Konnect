using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Messages;
using Kaenx.Konnect.Messages.Request;
using Kaenx.Konnect.Responses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Connections
{
    public interface IKnxConnection
    {
        public delegate void TunnelRequestHandler(Builders.TunnelResponse response);
        public event TunnelRequestHandler OnTunnelRequest;
        public event TunnelRequestHandler OnTunnelResponse;
        public event TunnelRequestHandler OnTunnelAck;

        public delegate void SearchResponseHandler(SearchResponse response);
        public event SearchResponseHandler OnSearchResponse;

        public delegate void ConnectionChangedHandler(bool isConnected);
        public event ConnectionChangedHandler ConnectionChanged;


        /// <summary>
        /// If you are connected to the Interface
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// Connects the interface to the bus.
        /// </summary>
        /// <returns></returns>
        Task Connect();

        /// <summary>
        /// Disconnects the interface from the bus
        /// </summary>
        /// <returns></returns>
        Task Disconnect();

        /// <summary>
        /// Checks the connection to the bus or keeps it alive
        /// </summary>
        /// <returns>Boolean if interface is connected to the bus (only USB)</returns>
        Task<bool> SendStatusReq();

        /// <summary>
        /// Sends the data to the bus.
        /// </summary>
        /// <param name="data">Data as byte array</param>
        /// <param name="ignoreConnected">If true the conected status will be ignored</param>
        /// <returns></returns>
        Task Send(byte[] data, bool ignoreConnected = false);

        /// <summary>
        /// Sends the data to the bus.
        /// </summary>
        /// <param name="message">Telegram message</param>
        /// <param name="igoreConnected">If true the conected status will be ignored</param>
        /// <returns>Sequenz Counter as byte</returns>
        Task<byte> Send(IMessageRequest message, bool ignoreConnected = false);
    }
}
