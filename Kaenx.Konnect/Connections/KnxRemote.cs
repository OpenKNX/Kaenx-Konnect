using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Interfaces;
using Kaenx.Konnect.Messages;
using Kaenx.Konnect.Messages.Request;
using Kaenx.Konnect.Messages.Response;
using Kaenx.Konnect.Remote;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Connections
{
    public class KnxRemote : IKnxConnection
    {
        public bool IsConnected { get; set; }
        public ConnectionErrors LastError { get; set; }
        public UnicastAddress PhysicalAddress { get; set; } = UnicastAddress.FromString("1.1.255");
        public int SequenceNumber
        {
            get { return _sequenceNumber; }
            set
            {
                _sequenceNumber = value;
                if (_sequenceNumber > 255)
                    _sequenceNumber = 0;
            }
        }

        public event IKnxConnection.TunnelRequestHandler OnTunnelRequest;
        public event IKnxConnection.TunnelResponseHandler OnTunnelResponse;
        public event IKnxConnection.TunnelAckHandler OnTunnelAck;
        public event IKnxConnection.SearchResponseHandler OnSearchResponse;
        public event IKnxConnection.ConnectionChangedHandler ConnectionChanged;

        private ClientWebSocket socket { get; set; } = new ClientWebSocket();
        private CancellationTokenSource ReceiveTokenSource { get; set; }
        private Dictionary<int, IRemoteMessage> Responses { get; set; } = new Dictionary<int, IRemoteMessage>();

        private int _sequenceNumber = 0;
        private int _connId = 0;
        private string Hash;
        private RemoteType Type;
        private RemoteConnection _conn;
        private IKnxConnection _knxConn;

        public KnxRemote(string hash, RemoteType type, RemoteConnection conn)
        {
            Hash = hash;
            Type = type;
            _conn = conn;

            _conn.OnRequest += _conn_OnRequest;
            _conn.OnResponse += _conn_OnResponse;

            for (int i = 0; i < 256; i++)
            {
                Responses.Add(i, null);
            }
        }

        private void _conn_OnResponse(IRemoteMessage message)
        {
            if (!(message is TunnelResponse)) return;
            TunnelResponse req = message as TunnelResponse;
            if (req.Type != TunnelTypes.Tunnel) return;

            IMessage msg = (IMessage)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(req.Data), new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });

            if(msg is IMessageRequest)
            {
                OnTunnelRequest?.Invoke(msg as IMessageRequest);
            }
            else
            {
                if (msg is MsgAckRes)
                    OnTunnelAck?.Invoke(msg as MsgAckRes);
                else
                    OnTunnelResponse?.Invoke(msg as IMessageResponse);
            }
        }

        private async void _conn_OnRequest(IRemoteMessage message)
        {
            if (!(message is TunnelRequest)) return;

            TunnelRequest req = message as TunnelRequest;
            if (req.ConnId != 0 && req.ConnId != _connId) return;

            switch (req.Type)
            {
                case TunnelTypes.Connect:
                    string hash = Encoding.UTF8.GetString(req.Data);
                    IKnxInterface inter = _conn.GetInterface(hash);


                    _knxConn = KnxInterfaceHelper.GetConnection(inter, _conn);
                    _knxConn.OnTunnelResponse += OnTunnelActivity;
                    _knxConn.OnTunnelRequest += OnTunnelActivity;


                    try
                    {
                        _connId = new Random().Next(1, 255);
                        await _knxConn.Connect();
                        TunnelResponse res = new TunnelResponse();
                        res.Type = TunnelTypes.Connect;
                        res.SequenceNumber = req.SequenceNumber;
                        res.Group = req.Group;
                        res.ChannelId = req.ChannelId;
                        res.ConnId = _connId;
                        _ = _conn.Send(res, false);
                    }
                    catch (Exception ex)
                    {
                        TunnelResponse res = new TunnelResponse();
                        res.Type = TunnelTypes.Connect;
                        res.Group = req.Group;
                        res.ChannelId = req.ChannelId;
                        res.ConnId = 0;
                        res.Data = Encoding.UTF8.GetBytes(ex.Message);
                        _ = _conn.Send(res, false);
                    }
                    break;

                case TunnelTypes.Tunnel:
                    IMessage msg = (IMessage)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(req.Data), new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                    byte seq = await _knxConn.Send(msg);

                    TunnelResponse req2 = new TunnelResponse(new byte[] { seq });
                    req2.Type = TunnelTypes.Response;
                    req2.ConnId = req.ConnId;
                    req2.SequenceNumber = req.SequenceNumber;
                    _ = _conn.Send(req2, false);
                    break;
            }
        }

        private void OnTunnelActivity(IMessage message)
        {
            Debug.WriteLine("Neue Tunnel Activity " + message.ApciType + "/" + BitConverter.ToString(message.Raw));
            TunnelResponse req = new TunnelResponse();
            req.Type = TunnelTypes.Tunnel;
            req.ConnId = _connId;
            req.Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All }));
            _ = _conn.Send(req, false);
        }

        public async Task Connect()
        {
            Debug.WriteLine(DateTime.Now);
            TunnelRequest req = new TunnelRequest(Encoding.UTF8.GetBytes(Hash));
            req.Type = TunnelTypes.Connect;
            IRemoteMessage resp = await _conn.Send(req);

            if (resp == null)
                throw new Exception("Keine Antwort vom Remote Server");

            TunnelResponse response = resp as TunnelResponse;
            if (response == null)
                throw new Exception("Unerwartete Antwort vom Remote Server: " + response.ToString());

            IsConnected = true;
            _connId = response.ConnId;
        }

        public async Task Disconnect()
        {


            IsConnected = false;
        }

        public Task Send(byte[] data, bool ignoreConnected = false)
        {
            throw new NotImplementedException();
        }

        public async Task<byte> Send(IMessage message, bool ignoreConnected = false)
        {
            if (!ignoreConnected && !IsConnected)
                throw new Exception("Roflkopter 2");

            TunnelRequest req = new TunnelRequest();
            req.Type = TunnelTypes.Tunnel;
            req.ConnId = _connId;
            req.Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All }));

            IRemoteMessage resp = await _conn.Send(req);

            if (resp is TunnelResponse)
            {
                return (resp as TunnelResponse).Data[0];
            }

            return 0;
        }

        public Task<bool> SendStatusReq()
        {
            throw new NotImplementedException();
        }






    }

    public enum RemoteType
    {
        Ip,
        Usb
    }
}
