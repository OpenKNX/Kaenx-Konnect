using Kaenx.Konnect.Remote;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Connections
{
    public class RemoteToServer
    {
        private ClientWebSocket socket { get; set; } = new ClientWebSocket();

        public readonly string Hostname;
        public readonly string Authentication;
        public string Group { get; set; }



        public delegate void MessageHandler(IRemoteMessage message);
        public event MessageHandler OnResponse;
        public event MessageHandler OnRequest;



        private CancellationTokenSource source = new CancellationTokenSource();
        private CancellationTokenSource ReceiveTokenSource { get; set; }
        private Dictionary<int, IRemoteMessage> Responses { get; set; } = new Dictionary<int, IRemoteMessage>();


        private int _sequenceNumber = 0;
        private byte _sequenceCounter = 0;
        private byte _communicationChannel;
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

        public RemoteToServer(string hostname, string authentication)
        {
            Hostname = hostname;
            Authentication = authentication;
            socket.Options.AddSubProtocol("chat");

            for (int i = 0; i < 256; i++)
            {
                Responses.Add(i, null);
            }
        }

        public async Task Connect()
        {
            //IsActive = true;

            await socket.ConnectAsync(new Uri("wss://" + Hostname), source.Token);
            int seq = SequenceNumber++;
            Debug.WriteLine("Sequenz: " + seq);
            AuthRequest msg = new AuthRequest(Authentication, seq);
            ReceiveTokenSource = new CancellationTokenSource();
            ProcessReceivingMessages();
            await socket.SendAsync(msg.GetBytes(), WebSocketMessageType.Binary, true, source.Token);
            IRemoteMessage resp = await WaitForResponse(seq);

            if(resp is StateMessage)
            {
                StateMessage response = (StateMessage)resp;
                switch (response.Code)
                {
                    case StateCodes.WrongKey:
                        throw new Exception("Authentifizierung am Server fehlgeschlagen");

                    case StateCodes.GroupNotFound:
                        throw new Exception("Angegebene Gruppe ist nicht auf dem Server vorhanden");

                    case StateCodes.WrongGroupKey:
                        throw new Exception("Authentifizierung in der Gruppe fehlgeschlagen");
                }
            } else if(resp is AuthResponse)
            {
                AuthResponse response = (AuthResponse)resp;
                Group = response.Code;
            }

            
        }

        public async Task<IRemoteMessage> Send(IRemoteMessage message, bool waitForResponse = true)
        {
            message.SequenceNumber = SequenceNumber++;
            message.ChannelId = ChannelId;
            await socket.SendAsync(message.GetBytes(), WebSocketMessageType.Binary, true, source.Token);
            IRemoteMessage mesg = await WaitForResponse(message.SequenceNumber);
            return mesg;
        }

        public async void Disconnect()
        {
            if(socket.State == WebSocketState.Open)
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Übertragung beendet", source.Token);
        }


        private async Task<IRemoteMessage> WaitForResponse(int seq)
        {
            Responses[seq] = null;
            CancellationTokenSource source = new CancellationTokenSource(10000);
            while (Responses[seq] == null && !source.Token.IsCancellationRequested)
                await Task.Delay(10);

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

                    }

                    if (result == null || result.Count == 0) continue;

                    Kaenx.Konnect.Remote.MessageCodes code = (Konnect.Remote.MessageCodes)buffer.Array[0];

                    Debug.WriteLine("Typ: " + code);

                    var q = from t in Assembly.LoadFrom("Kaenx.Konnect.dll").GetTypes()
                            where t.IsClass && t.IsNested == false && t.Namespace == "Kaenx.Konnect.Remote"
                            select t;

                    IRemoteMessage message = null;

                    foreach (Type t in q.ToList())
                    {
                        IRemoteMessage down = (IRemoteMessage)Activator.CreateInstance(t);
                        if (code == down.MessageCode)
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
                    }catch(Exception ex)
                    {

                    }


                    if (message.ToString().EndsWith("Response"))
                    {
                        Responses[message.SequenceNumber] = message;
                        OnResponse?.Invoke(message);
                    } else
                    {
                        OnRequest?.Invoke(message);
                    }



                    Debug.WriteLine("Neue Nachricht: " + message + " / " + message.SequenceNumber);
                }

                Debug.WriteLine("Verbindung abgebrochen");
            });
        }
    }
}
