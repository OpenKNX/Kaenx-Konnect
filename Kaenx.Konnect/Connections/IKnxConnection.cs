using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Messages;
using Kaenx.Konnect.Messages.Request;
using Kaenx.Konnect.Messages.Response;
using Kaenx.Konnect.Responses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Connections
{
    public interface IKnxConnection : IDisposable
    {
        public delegate void TunnelRequestHandler(IMessageRequest message);
        public event TunnelRequestHandler OnTunnelRequest;
        public delegate void TunnelResponseHandler(IMessageResponse message);
        public event TunnelResponseHandler OnTunnelResponse;
        public delegate void TunnelAckHandler(MsgAckRes message);
        public event TunnelAckHandler OnTunnelAck;

        public delegate void ConnectionChangedHandler(bool isConnected);
        public event ConnectionChangedHandler ConnectionChanged;


        /// <summary>
        /// If you are connected to the Interface
        /// </summary>
        public bool IsConnected { get; set; }
        
        /// <summary>
        /// Returns Last Error
        /// </summary>
        public ConnectionErrors LastError { get; set; }

        /// <summary>
        /// Returns the Physical Address of the Interface
        /// </summary>
        public UnicastAddress? PhysicalAddress { get; set; }

        /// <summary>
        /// Returns the max APDU length of the interface
        /// </summary>
        public int MaxFrameLength { get; set; }

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
        Task<byte> Send(IMessage message, bool ignoreConnected = false);

        /// <summary>
        /// Send Acknowledge to the bus
        /// </summary>
        /// <param name="sequenceNumber">Sequence Number</param>
        //Task<byte> SendAck(byte sequenceNumber);
    }


    public enum ConnectionErrors
    {
        Undefined = 1,
        NoMoreConnections,
        NotConnectedToBus
    }
}
