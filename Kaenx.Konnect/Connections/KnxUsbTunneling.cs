using Device.Net;
using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Builders;
using Kaenx.Konnect.Messages;
using Kaenx.Konnect.Messages.Request;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Kaenx.Konnect.Connections.IKnxConnection;

namespace Kaenx.Konnect.Connections
{
    public class KnxUsbTunneling : IKnxConnection
    {
        public bool IsConnected { get; set; }

        public event TunnelRequestHandler OnTunnelRequest;
        public event TunnelResponseHandler OnTunnelResponse;
        public event TunnelAckHandler OnTunnelAck;
        public event SearchResponseHandler OnSearchResponse;
        public event SearchRequestHandler OnSearchRequest;
        public event ConnectionChangedHandler ConnectionChanged;

        private readonly BlockingCollection<object> _sendMessages;
        private IDevice DeviceKnx { get; }
        private ProtocolTypes CurrentType { get; set; }
        public ConnectionErrors LastError { get; set; }
        public UnicastAddress PhysicalAddress { get; set; }


        public KnxUsbTunneling(IDevice device)
        {
            DeviceKnx = device;
            _sendMessages = new BlockingCollection<object>();
        }


        //TODO Adresse der Schnitstelle schreibt man im Speicher an Adresse 279

        public async Task Connect()
        {
            byte[] packet = new byte[64]; //Create packet which will be fixed 64 bytes long

            //Report Header
            packet[0] = 1; //Report Id (fixed)
            packet[1] = 0x13; // First Packet with start and end
            packet[2] = 9; // Data length

            //Transfer Header
            packet[3] = 0; // Version fixed 0
            packet[4] = 8; // Header length
            packet[5] = 0; // Body length
            packet[6] = 1; // Body length
            packet[7] = 0x0F; // Protocol ID
            packet[8] = 0x01; // Service Identifier
            packet[9] = 0; // Manufacturer Code
            packet[10] = 0; // Manufacturer Code

            // Body
            packet[11] = 1; // Supported Emis

            bool isInited = DeviceKnx.IsInitialized;

            if (!isInited)
            {
                try
                {
                    await DeviceKnx.InitializeAsync();
                }
                catch
                {
                    throw new Exception("Es konnte keine Verbindung mit dem USB Gerät hergestellt werden.");
                }
            }

            CancellationTokenSource source = new CancellationTokenSource(1000);

            TransferResult read = await DeviceKnx.WriteAndReadAsync(packet, source.Token);

            BitArray emis = new BitArray(new byte[] { read.Data[13] });

            if (emis.Get(2))
            {
                Debug.WriteLine("USB: Verwende cEmi");
                CurrentType = ProtocolTypes.cEmi;
            } else if (emis.Get(1))
            {
                Debug.WriteLine("USB: Verwende Emi2");
                CurrentType = ProtocolTypes.Emi2;
            } else if (emis.Get(0))
            {
                CurrentType = ProtocolTypes.Emi1;
                Debug.WriteLine("USB: Verwende Emi1");
            } else
            {
                Debug.WriteLine("USB: unterstützt kein Emi1/Emi2/cEmi");
            }


            packet[6] = 0x02;
            packet[8] = 0x03; //Device Feature Set
            packet[11] = 0x05; //Set Emi Type
            packet[12] = Convert.ToByte(CurrentType); //Selected Emi type

            source = new CancellationTokenSource(1000);
            await DeviceKnx.WriteAsync(packet, source.Token);

            packet[6] = 0x01;
            packet[8] = 0x01; //Device Feature Get
            packet[11] = 0x03; //Bus Status
            packet[12] = 0x00;

            source = new CancellationTokenSource(1000);
            read = await DeviceKnx.WriteAndReadAsync(packet, source.Token);

            int status = read.Data[12];

            if(status == 0)
            {
                throw new Exception("Schnittstelle hat keine Verbindung zum Bus.");
            }


            packet[2] = 0x0c;
            packet[6] = 0x04; //Body length
            packet[7] = 0x01; //Tunnel
            packet[8] = Convert.ToByte(CurrentType); //Selected Emi
            packet[11] = 0x4c; //Memory Read
            packet[12] = 0x02; //Length
            packet[13] = 0x01; //Address High
            packet[14] = 0x17; //Address Low

            source = new CancellationTokenSource(1000);
            read = await DeviceKnx.WriteAndReadAsync(packet, source.Token);

            PhysicalAddress = UnicastAddress.FromByteArray(new byte[] { read.Data[15], read.Data[16] });

            if (source.IsCancellationRequested)
            {
                DeviceKnx.Close();
                throw new Exception("Schnittstelle antwortet nicht.");
            }

            IsConnected = true;
            ConnectionChanged?.Invoke(true);

            ProcessReceiveMessages();
            ProcessSendMessages();
        }

        public async Task Disconnect()
        {
            DeviceKnx.Close();
            DeviceKnx.Dispose();
        }

        public async Task Send(byte[] data, bool ignoreConnected = false)
        {
            throw new NotImplementedException();
        }

        public async Task<byte> Send(IMessage message, bool ignoreConnected = false)
        {
            if (!ignoreConnected && !IsConnected)
                throw new Exception("Roflkopter");

            _sendMessages.Add(message);

            return 0x01;
        }

        public async Task<bool> SendStatusReq()
        {
            byte[] packet = new byte[64]; //Create packet which will be fixed 64 bytes long

            //Report Header
            packet[0] = 1; // Report ID fixed 1;
            packet[1] = 0x13; // First Packet with start and end
            packet[2] = 9; // Data length


            //Transfer Header
            packet[3] = 0; // Version fixed 0
            packet[4] = 8; // Header length
            packet[5] = 0; // Body length
            packet[6] = 1; // Body length
            packet[7] = 0x0F; // Protocol ID
            packet[8] = 0x01; // Service Identifier
            packet[9] = 0; // Manufacturer Code
            packet[10] = 0; // Manufacturer Code

            // Body
            packet[11] = 3; // Bus Status

            await DeviceKnx.InitializeAsync();

            await DeviceKnx.WriteAsync(packet);

            DeviceKnx.Close();

            return true;
        }

        private void ProcessReceiveMessages()
        {
            Task.Run(async () =>
            {
                while(true)
                {
                    try
                    {
                        //CancellationTokenSource source = new CancellationTokenSource(1000);
                        TransferResult res = await DeviceKnx.ReadAsync(); // source.Token);
                    }
                    catch
                    {

                    }
                }
            });
        }


        private void ProcessSendMessages()
        {
            Task.Run(async () =>
            {
                while(true)
                {
                    foreach (var sendMessage in _sendMessages.GetConsumingEnumerable())
                    {
                        try
                        {
                            if (sendMessage is byte[])
                            {


                            }
                            else if (sendMessage is MsgSearchReq)
                            {
                                return; //SearchRequest not supported on USB
                            }
                            else if (sendMessage is IMessage)
                            {
                                IMessage message = sendMessage as IMessage;
                                List<byte> xdata = new List<byte>();

                                //Report Header
                                xdata.Add(0x01); //Report ID fixed 1
                                xdata.Add(0x13); //First Packet with start and end
                                xdata.Add(0x09); //Data Length

                                //Transfer Header
                                xdata.Add(0x00); //Protocoll Version
                                xdata.Add(0x08); //Header Length fixed
                                xdata.Add(0x00); //Body Length
                                xdata.Add(0x08); //Body Length
                                xdata.Add(0x01); //Protocol ID 1=KNX Tunnel
                                xdata.Add(0x01); //EMI Version 1=EMI1
                                xdata.Add(0x00); //Manufacturer Code
                                xdata.Add(0x00); //Manufacturer Code


                                byte[] body;

                                switch (CurrentType)
                                {
                                    case ProtocolTypes.Emi1:
                                        body = message.GetBytesEmi1();
                                        break;

                                    case ProtocolTypes.Emi2:
                                        body = message.GetBytesEmi1(); //Todo check diffrences between emi1
                                                                       //xdata.AddRange(message.GetBytesEmi2());
                                        break;

                                    case ProtocolTypes.cEmi:
                                        body = message.GetBytesCemi();
                                        break;

                                    default:
                                        throw new Exception("Unbekanntes Protokoll");
                                }

                                xdata[2] = (byte)(body.Length + xdata[4] - 1);
                                xdata[6] = (byte)(body.Length - 1);

                                xdata.AddRange(body);



                                int toadd = (64 - xdata.Count);
                                for (int i = 0; i < toadd; i++)
                                    xdata.Add(0x00);

                                CancellationTokenSource source = new CancellationTokenSource(1000);

                                await DeviceKnx.WriteAsync(xdata.ToArray(), source.Token);
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            });
        }
    }
}
