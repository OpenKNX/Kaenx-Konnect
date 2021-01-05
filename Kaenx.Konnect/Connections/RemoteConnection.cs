using Kaenx.Konnect.Connections;
using Kaenx.Konnect.Interfaces;
using Kaenx.Konnect.Remote;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Connections
{
    public class RemoteConnection : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        private CancellationTokenSource source = new CancellationTokenSource();
        private CancellationTokenSource ReceiveTokenSource { get; set; }
        private Dictionary<int, IRemoteMessage> Responses { get; set; } = new Dictionary<int, IRemoteMessage>();

        private ClientWebSocket socket { get; set; } = new ClientWebSocket();

        public delegate void MessageHandler(IRemoteMessage message);
        public event MessageHandler OnRequest;
        public event MessageHandler OnResponse;


        public delegate IKnxInterface RequestHandler(string hash);
        public event RequestHandler OnRequestInterface;


        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; Changed("IsActive"); }
        }

        private bool _isConnected = false;
        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; Changed("IsConnected"); }
        }


        private string _state = "Getrennt";
        public string State
        {
            get { return _state; }
            set { _state = value; Changed("State"); }
        }


        private int _sequenceNumber = 0;
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
        public int ChannelId { get; set; }
        public string Group { get; set; }
        public string GroupOut { get; set; }
        public string Hostname { get; private set; }
        public string Authentification { get; private set; }


        public RemoteConnection()
        {
            socket.Options.AddSubProtocol("chat");

            for (int i = 0; i < 256; i++)
            {
                Responses.Add(i, null);
            }
        }


        public async Task Connect(string host, string auth, bool isSecure, string group = null, string code = null)
        {
            Hostname = host;
            Authentification = auth;
            State = "Verbinde...";
            IsActive = true;

            try
            {
                socket = new ClientWebSocket();
                socket.Options.AddSubProtocol("chat");
                await socket.ConnectAsync(new Uri((isSecure ? "wss://":"ws://") + host), source.Token);
                int seq = SequenceNumber++;
                AuthRequest msg = new AuthRequest(auth, seq, group, code);
                ReceiveTokenSource = new CancellationTokenSource();
                ProcessReceivingMessages();
                await socket.SendAsync(msg.GetBytes(), WebSocketMessageType.Binary, true, source.Token);
                IRemoteMessage resp = await WaitForResponse(seq);

                if (resp is StateResponse)
                {
                    StateResponse response = (StateResponse)resp;
                    switch (response.Code)
                    {
                        case StateCodes.WrongKey:
                            throw new Exception("Authentifizierung am Server fehlgeschlagen");

                        case StateCodes.GroupNotFound:
                            throw new Exception("Angegebene Gruppe ist nicht auf dem Server vorhanden");

                        case StateCodes.WrongGroupKey:
                            throw new Exception("Authentifizierung in der Gruppe fehlgeschlagen");
                    }
                }
                else if (resp is AuthResponse)
                {
                    AuthResponse response = (AuthResponse)resp;
                    Group = response.Group;
                }
                State = "Verbunden (" + Group + ")";
            } catch(Exception ex)
            {
                State = ex.Message;
                IsActive = false;
                IsConnected = false;
            }
        }

        public async Task<IRemoteMessage> Send(IRemoteMessage message, bool waitForResponse = true)
        {
            if (socket == null || socket.State != WebSocketState.Open)
            {
                return null;
            }
            IRemoteMessage mesg = null;

            try
            {
                if(message.SequenceNumber == -1) message.SequenceNumber = SequenceNumber++;
                if(message.ChannelId == 0) message.ChannelId = ChannelId;
                if (message is TunnelRequest)
                {
                    TunnelRequest req = message as TunnelRequest;
                    if (req.Group == "") req.Group = GroupOut;
                }
                if (message is TunnelResponse)
                {
                    TunnelResponse res = message as TunnelResponse;
                    if (res.Group == "") res.Group = GroupOut;
                }
                await socket.SendAsync(message.GetBytes(), WebSocketMessageType.Binary, true, source.Token);
                mesg = await WaitForResponse(message.SequenceNumber);
            }catch(Exception ex)
            {

            }

            return mesg;
        }

        public async Task Disconnect()
        {
            if ( socket != null && socket.State == WebSocketState.Open)
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Übertragung beendet", source.Token);

            IsActive = false;
            State = "Getrennt";
        }


        private async Task<IRemoteMessage> WaitForResponse(int seq)
        {
            Responses[seq] = null;
            CancellationTokenSource source = new CancellationTokenSource(10000);
            while (Responses[seq] == null && !source.Token.IsCancellationRequested)
                await Task.Delay(1000);

            return Responses[seq];
        }


        private void ProcessReceivingMessages()
        {
            Task.Run(async () =>
            {
                while (socket.State == WebSocketState.Open)
                {
                    ArraySegment<byte> buffer;
                    WebSocketReceiveResult result = null;
                    try
                    {
                        buffer = new ArraySegment<byte>(new byte[1024]);
                        result = await socket.ReceiveAsync(buffer, ReceiveTokenSource.Token);
                    }
                    catch (Exception ex)
                    {
                        socket = null;
                        IsActive = false;
                        IsConnected = false;
                        return;
                    }

                    if (result == null || result.Count == 0) continue;

                    Kaenx.Konnect.Remote.MessageCodes code = (Konnect.Remote.MessageCodes)buffer.Array[0];

                    Debug.WriteLine("Got Remote Message: " + code);

                    //Check if assemby from this genügt
                    var q = from t in Assembly.LoadFrom("Kaenx.Konnect.dll").GetTypes()
                            where t.IsClass && t.IsNested == false && t.Namespace == "Kaenx.Konnect.Remote"
                            select t;

                    IRemoteMessage message = null;

                    foreach (Type t in q.ToList())
                    {
                        IRemoteMessage down = (IRemoteMessage)Activator.CreateInstance(t);
                        if (down != null && code == down.MessageCode)
                        {
                            message = down;
                            break;
                        }
                    }

                    if (message == null)
                    {
                        Debug.WriteLine("Unbekannte Nachricht: " + code);
                    }
                    try
                    {
                        message.Parse(buffer.Array.Take(result.Count).ToArray());
                    }
                    catch (Exception ex)
                    {

                    }

                    if(message is TunnelResponse)
                    {

                    }


                    if (message.ToString().EndsWith("Response"))
                    {
                        Responses[message.SequenceNumber] = message;
                        OnResponse?.Invoke(message);
                    }
                    else
                    {
                        OnRequest?.Invoke(message);
                    }
                }

                Debug.WriteLine("Verbindung abgebrochen");
            });
        }


        public IKnxInterface GetInterface(string hash)
        {
            IKnxInterface inter = OnRequestInterface?.Invoke(hash);
            return inter;
        }



        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
