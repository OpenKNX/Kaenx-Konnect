
using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Classes.Helper;
using Kaenx.Konnect.Connections;
using Kaenx.Konnect.EMI.DataMessages;
using Kaenx.Konnect.EMI.LData;
using Kaenx.Konnect.Enums;
using Kaenx.Konnect.Exceptions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Kaenx.Konnect.Classes
{
    public class BusDevice
    {
        private string _mask = "";

        public ManagementModels ManagmentModel { get; set; } = ManagementModels.None;
        public int MaxFrameLength { get; set; } = 15;
        public ushort? MaskVersion { get; private set; } = null;

        private bool _isIndividual = false;
        private bool _isConnected = false;
        private UnicastAddress _address;
        private IKnxConnection _conn;
        private Dictionary<int, ResponseHelper> responses = new Dictionary<int, ResponseHelper>();
        private Dictionary<int, CancellationTokenSource?> acks = new Dictionary<int, CancellationTokenSource?>();
        private Dictionary<string, string> features = new Dictionary<string, string>();
        private int timeoutForData = 3000;

        private byte _seqNum = 0;
        private byte _currentSeqNum
        {
            get
            {
                return _isIndividual ? (byte)255 : _seqNum;
            }
            
            set
            {
                _seqNum = value;
                if (_seqNum > 15)
                {
                    _seqNum = 0;
                }
            }
        }

        private VerifyMode verifyMode = VerifyMode.Unknown;

        public BusDevice(string address, IKnxConnection conn)
        {
            _address = UnicastAddress.FromString(address);
            _conn = conn;
        }
        public BusDevice(UnicastAddress address, IKnxConnection conn)
        {
            _address = address;
            _conn = conn;
        }

        public void SetTimeout(int timeout)
        {
            timeoutForData = timeout;
        }

        public bool IsConnected()
        {
            return _isConnected;
        }

        #region Waiters
        private async Task<IDataMessage> WaitForData(IDataMessage message, byte sequenceNumber = 255)
        {
            return await WaitForData<IDataMessage>(message, sequenceNumber);
        }

        private async Task<T> WaitForData<T>(IDataMessage message, byte sequenceNumber = 255)
        {
            if (responses.ContainsKey(sequenceNumber))
                responses.Remove(sequenceNumber);

            ResponseHelper helper = new ResponseHelper();
            responses.Add(sequenceNumber, helper);

            Debug.WriteLine($"Bus Device | Send  Dat: {sequenceNumber} | {message.GetType().FullName}");

            if(!_isIndividual)
            {
                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        await WaitForAck(message, sequenceNumber);
                        Debug.WriteLine($"Bus Device | Send Got Ack: {sequenceNumber}");
                        // We got an Ack
                        break;
                    }
                    catch (TaskCanceledException)
                    {
                        Debug.WriteLine($"Bus Device | Send Got no Ack: {sequenceNumber}");
                        // We got no Ack for sending data!
                    }
                    catch (TimeoutException)
                    {
                        Debug.WriteLine($"Bus Device | Send Timeout: {sequenceNumber}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Got Exception: " + ex.Message);
                        throw new Exception("Could not wait for Ack", ex);
                    }
                }
            } else
            {
                LDataBase lDataBase = new(_address, sequenceNumber != 255, sequenceNumber, message);
                await _conn.SendAsync(lDataBase);
            }

            try {
                await Task.Delay(timeoutForData, helper.TokenSource.Token);
                Debug.WriteLine($"Bus Device | Try   Dat: {sequenceNumber}");
            } catch {
                Debug.WriteLine($"Bus Device | Catch Dat: {sequenceNumber}");
                // If the Token was cancelled, we got the Response
            }

            responses.Remove(sequenceNumber);
            if(helper.Response == null)
                throw new TimeoutException("Zeitüberschreitung beim Warten auf Antwort");

            await Task.Delay(10);

            try
            {
                return (T)Convert.ChangeType(helper.Response, typeof(T));
            }
            catch (Exception e)
            {
                throw new Exception($"IDataMessage kann nicht konvertieren: {helper.Response.GetType().Name} -> {typeof(T).Name}", e);
            }
        }

        private async Task WaitForAck(IDataMessage message, byte sequenceNumber, bool increaseSequence = true)
        {
            if (acks.ContainsKey(sequenceNumber))
                acks.Remove(sequenceNumber);


            bool isNumbered = true;
            if (sequenceNumber == 255)
            {
                sequenceNumber = 0;
                isNumbered = false;
            }


            CancellationTokenSource tokenS = new CancellationTokenSource();
            acks.Add(sequenceNumber, tokenS);
            //Debug.WriteLine("Bus Device | Send  Ack: " + message.SequenceNumber);
            LDataBase lDataBase = new(_address, isNumbered, sequenceNumber, message);
            await _conn.SendAsync(lDataBase);

            bool gotAck = true;

            if(!_isIndividual)
            {
                try
                {
                    await Task.Delay(timeoutForData, tokenS.Token);
                    gotAck = false;
                    //Debug.WriteLine("Bus Device | Try   Ack: " + message.SequenceNumber);
                }
                catch
                {
                    //Debug.WriteLine("Bus Device | Catch Ack: " + message.SequenceNumber);
                    // If the Token was cancelled, we got the Ack
                }
            }

            acks.Remove(sequenceNumber);
            if (!gotAck)
                throw new TimeoutException("Zeitüberschreitung beim Warten auf Ack");

            if (gotAck && increaseSequence)
                _currentSeqNum++;
        }
        #endregion


        #region Helper Functions
        public async Task InterfaceReset()
        {
            await _conn.Disconnect();
            await _conn.Connect();
        }

        public void SetMaxFrameLength(int maxFrameLength)
        {
            MaxFrameLength = maxFrameLength;
        }

        private async Task<string> GetMaskVersion()
        {
            if (_mask != "") return _mask;

            _mask = await DeviceDescriptorRead();
            return _mask;
        }


        /// <summary>
        /// Checks if the Device is reachable. 
        /// May not work with older devices.
        /// </summary>
        /// <returns>Bool if device is reachable</returns>
        public async Task<bool> IsReachable()
        {
            try
            {
                int res = await PropertyRead<int>(0, 1);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion


        #region Connection

        public async Task ConnectIndividual(bool onlyConnect = false)
        {
            _isIndividual = true;

            await Connect(onlyConnect);
        }

        /// <summary>
        /// Stellt eine Verbindung mit dem Gerät her.
        /// Wird für viele weitere Methoden benötigt.
        /// </summary>
        public async Task Connect(bool onlyConnect = false)
        {
            if(_isConnected && !_isIndividual)
                await Disconnect(); //reset the connection

            _currentSeqNum = 0;

            _conn.OnReceivedMessage += OnTunnelResponse;

            if(!_isIndividual)
            {
                LDataBase request = new LDataBase(_address, false, _currentSeqNum, new Connect());
                await _conn.SendAsync(request);
            }

            _isConnected = true;

            if (onlyConnect)
            {
                MaxFrameLength = 15;
                return;
            }

            try
            {
                MaxFrameLength = await PropertyRead<short>(0, 56);
                //Debug.WriteLine("Maximale Länge:  " + MaxFrameLength);
                if (MaxFrameLength < 15) MaxFrameLength = 15;
                //Debug.WriteLine("Maximale Länge*: " + MaxFrameLength);
                await GetMaskVersion();
            }
            catch
            {
                MaxFrameLength = 15;
                Debug.WriteLine("Bus Device | Gerät hat die Property MaxAPDU nicht. Es wird von 15 ausgegangen");
                if(!_isIndividual)
                {
                    await Disconnect();
                    await Connect(true);
                }
            }
        }

        private async void OnTunnelResponse(LDataBase message)
        {
            if(message.IsControl)
            {
                switch(message.GetApciType())
                {
                    case ApciTypes.Ack:
                        if (acks.ContainsKey(message.SequenceNumber))
                            acks[message.SequenceNumber]?.Cancel();
                        break;

                    case ApciTypes.Disconnect:
                        _isConnected = false;
                        break;
                }
            } else
            {
                if (message.Content == null)
                    return;
                    
                if (message.Content.ApciType == ApciTypes.GroupValueRead ||
                   message.Content.ApciType == ApciTypes.GroupValueWrite ||
                   message.Content.ApciType == ApciTypes.GroupValueResponse)
                {
                    // Ignore Group Messages
                    return;
                }

                if (message.Content.GetType().Name.EndsWith("Response"))
                {
                    if(!_isIndividual)
                    {
                        try
                        {
                            LDataBase messageAck = new LDataBase((UnicastAddress)message.SourceAddress, message.IsNumbered, message.SequenceNumber, new Ack());
                            await _conn.SendAsync(messageAck);
                        }
                        catch (Exception ex)
                        {
                            //throw new Exception("Device sent answer but no Ack received", ex);
                            return;
                        }
                    }
                    
                    if (responses.ContainsKey(message.SequenceNumber))
                    {
                        responses[message.SequenceNumber].Response = message.Content;
                        responses[message.SequenceNumber].TokenSource.Cancel();
                    }
                }
            }
        }

        /// <summary>
        /// Trennt die Verbindung zum Gerät
        /// </summary>
        public async Task Disconnect()
        {
            _isConnected = false;
            _isIndividual = false;

            LDataBase message = new LDataBase(_address, false, 0, new Disconnect());
            await _conn.SendAsync(message);

            _currentSeqNum = 0;

            _conn.OnReceivedMessage -= OnTunnelResponse;
        }

        /// <summary>
        /// Startet das Gerät neu.
        /// </summary>
        public async Task Restart()
        {
            if(!_isConnected && !_isIndividual)
                throw new DeviceNotConnectedException();

            EMI.DataMessages.Restart message = new EMI.DataMessages.Restart();

            bool isNumbered = true;
            byte sequenceNumber = _currentSeqNum;
            if (sequenceNumber == 255)
            {
                sequenceNumber = 0;
                isNumbered = false;
            }

            LDataBase lDataBase = new(_address, isNumbered, sequenceNumber, message);
            await _conn.SendAsync(lDataBase);
        }
        #endregion


        #region Resource
        /// <summary>
        /// Schreibe den Wert in die Property des Gerätes
        /// </summary>
        /// <param name="maskId">Id der Maske (z.B. MV-0701)</param>
        /// <param name="resourceId">Name der Ressource (z.B. ApplicationId)</param>
        /// <returns></returns>
        /// <exception cref="Kaenx.Konnect.Exceptions.NotSupportedException">Wenn Gerät Ressource nicht unterstützt</exception>
        public async Task ResourceWrite(string resourceId, byte[] data, uint startIndex = 1, uint count = 1)
        {
            if(!_isConnected && !_isIndividual)
                throw new DeviceNotConnectedException();

            string maskId = await GetMaskVersion();
            DeviceResource resource = ResourcenHelper.GetDeviceResource(resourceId, maskId);

            if (data.Length > resource.Length)
            {
                data = data.Skip(data.Length - resource.Length).ToArray();
            }

            switch (resource.AddressSpace)
            {
                case AddressSpace.SystemProperty:
                    await PropertyWrite(resource.InterfaceObjectRef, resource.PropertyID, data, startIndex, count);
                    break;

                case AddressSpace.StandardMemory:
                    await MemoryWrite(resource.Address, data);
                    break;

                case AddressSpace.RelativeMemoy:
                    uint addr = await PropertyRead<uint>(resource.InterfaceObjectRef, resource.PropertyID);
                    await MemoryWrite(addr, data);
                    break;

                default:
                    throw new Exception("AddressSpace not found or unknown");
            }
        }

        /// <summary>
        /// Schreibe den Wert in die Property des Gerätes und gibt die Antwort zurück
        /// </summary>
        /// <param name="objIdx">ObjektIndex</param>
        /// <param name="propId">PropertyId</param>
        /// <param name="data">Daten die geschrieben werden sollen</param>
        /// <returns>Property Wert</returns>
        /// <exception cref="System.TimeoutException" />
        public Task<byte[]> PropertyWriteResponse(byte objIdx, byte propId, byte[] data)
        {
            return PropertyWriteResponse<byte[]>(objIdx, propId, data);
        }

        /// <summary>
        /// Schreibe den Wert in die Property des Gerätes und gibt die Antwort zurück
        /// </summary>
        /// <param name="objIdx">ObjektIndex</param>
        /// <param name="propId">PropertyId</param>
        /// <param name="data">Daten die geschrieben werden sollen</param>
        /// <returns>Property Wert</returns>
        /// <exception cref="System.TimeoutException" />
        public async Task<T> PropertyWriteResponse<T>(byte objIdx, byte propId, byte[] data)
        {
            if(!_isConnected && !_isIndividual)
                throw new DeviceNotConnectedException();

            // TODO implement PropertyWriteResponse Message
            //MsgPropertyWriteReq message = new MsgPropertyWriteReq(objIdx, propId, data, _address);
            //message.SequenceNumber = _currentSeqNum;
            //MsgPropertyReadRes resp = new MsgPropertyReadRes(); // = (MsgPropertyReadRes)await WaitForData(message);
            return ConvertRawData<T>(new byte[0]);
        }


        /// <summary>
        /// Liest Property vom Gerät aus.
        /// </summary>
        /// <param name="maskId">Id der Maske (z.B. MV-0701)</param>
        /// <param name="resourceId">Name der Ressource (z.B. ApplicationId)</param>
        /// <returns>Property Wert as Byte Array</returns>
        /// <exception cref="Kaenx.Konnect.Exceptions.NotSupportedException">Wenn Gerät Ressource nicht unterstützt</exception>
        /// <exception cref="System.TimeoutException">Wenn Gerät Ressource nicht in angemessener Zeit antwortet</exception>
        public async Task<byte[]> ResourceRead(string resourceId, bool onlyAddress = false)
        {
            return await ResourceRead<byte[]>(resourceId, onlyAddress);
        }

        /// <summary>
        /// Liest Property vom Gerät aus.
        /// </summary>
        /// <param name="maskId">Id der Maske (z.B. MV-0701)</param>
        /// <param name="resourceId">Name der Ressource (z.B. ApplicationId)</param>
        /// <returns>Property Wert </returns>
        /// <exception cref="Kaenx.Konnect.Exceptions.NotSupportedException">Wenn Gerät Ressource nicht unterstützt</exception>
        public async Task<T> ResourceRead<T>(string resourceId, bool onlyAddress = false)
        {
            if(!_isConnected && !_isIndividual)
                throw new DeviceNotConnectedException();

            string maskId = await GetMaskVersion();
            DeviceResource resource = ResourcenHelper.GetDeviceResource(resourceId, maskId);

            switch (resource.AddressSpace)
            {
                case AddressSpace.SystemProperty:
                    return await PropertyRead<T>(resource.InterfaceObjectRef, resource.PropertyID);

                case AddressSpace.StandardMemory:
                    if (onlyAddress)
                        return (T)Convert.ChangeType(resource.Address, typeof(T));
                    else
                        return await MemoryRead<T>(resource.Address, resource.Length);

                case AddressSpace.RelativeMemoy:
                    int addr = await PropertyRead<int>(resource.InterfaceObjectRef, resource.PropertyID);
                    byte[] data = await MemoryRead((uint)addr, resource.Length);
                    return (T)Convert.ChangeType(data, typeof(T));
            }

            //TODO property aus knx_master auslesen
            object? objt = Convert.ChangeType(null, typeof(T));
            if(objt == null)
                throw new Exception("Data kann nicht in angegebenen Type konvertiert werden. " + typeof(T).ToString());
            return (T)objt;
        }
        #endregion


        #region Property
        /// <summary>
        /// Liest Property vom Gerät aus.
        /// </summary>
        /// <param name="objIdx">Objekt Index</param>
        /// <param name="propId">Property Id</param>
        /// <param name="length">Anzahl der zu lesenden Bytes</param>
        /// <param name="start">Startindex</param>
        /// <returns>Property Wert</returns>
        public async Task<byte[]> PropertyRead(uint objIdx, uint propId, uint start = 1, uint count = 1)
        {
            return await PropertyRead<byte[]>(objIdx, propId, count, start);
        }

        /// <summary>
        /// Liest Property vom Gerät aus.
        /// </summary>
        /// <param name="objIdx">Objekt Index</param>
        /// <param name="propId">Property Id</param>
        /// <param name="length">Anzahl der zu lesenden Bytes</param>
        /// <param name="start">Startindex</param>
        /// <returns>Property Wert</returns>
        /// <exception cref="System.TimeoutException" />
        public async Task<T> PropertyRead<T>(uint objIdx, uint propId, uint start = 1, uint count = 1)
        {
            if(!_isConnected && !_isIndividual)
                throw new DeviceNotConnectedException();

            if(objIdx > 255 || propId > 255)
            {
                throw new NotImplementedException("PropertyExtendedRead is not implemented");
            } else
            {
                PropertyValueResponse response = await WaitForData<PropertyValueResponse>(new PropertyValueRead(objIdx, propId, start, count), _currentSeqNum);
                return ConvertRawData<T>(response.Data);
            }
        }

        /// <summary>
        /// Liest Property Description vom Gerät aus.
        /// </summary>
        /// <param name="objIdx">Objekt Index</param>
        /// <param name="propId">Property Id</param>
        /// <returns>Direkte Antwort vom Gerät</returns>
        /// <exception cref="System.TimeoutException" />
        public async Task<PropertyDescriptionResponse> PropertyDescriptionRead(uint objIdx, uint propId, uint propIndex = 0)
        {
            if(!_isConnected && !_isIndividual)
                throw new DeviceNotConnectedException();

            PropertyDescriptionResponse response = await WaitForData<PropertyDescriptionResponse>(new PropertyDescriptionRead(objIdx, propId, propIndex), _currentSeqNum);
            return response;
        }



        /// <summary>
        /// Schreibe den Wert in die Property des Gerätes
        /// </summary>
        /// <param name="objIdx">ObjektIndex</param>
        /// <param name="propId">PropertyId</param>
        /// <param name="data">Daten die geschrieben werden sollen</param>
        /// <param name="waitForResp">Gibt an, ob auf eine Antwort gewartet werden soll</param>
        /// <returns></returns>
        /// <exception cref="System.TimeoutException" />
        public async Task<PropertyValueResponse?> PropertyWrite(uint objIdx, uint propId, byte[] data, uint startIndex = 1, uint count = 1, bool waitForResp = false)
        {
            if (!_isConnected && !_isIndividual)
                throw new DeviceNotConnectedException();

            var message = new PropertyValueWrite(objIdx, propId, startIndex, count, data);

            // TODO also make for that the response will be returned
            if (waitForResp)
                return await WaitForData<PropertyValueResponse>(message, _currentSeqNum);
            
            await WaitForAck(message, _currentSeqNum);
            return null;
        }

        /// <summary>
        /// Führt eine Function Propety 
        /// </summary>
        /// <param name="objIdx">ObjektIndex</param>
        /// <param name="propId">PropertyId</param>
        /// <param name="data">Daten die übergeben werden sollen</param>
        /// <returns></returns>
        /// <exception cref="System.TimeoutException" />
        public async Task<FunctionPropertyStateResponse> InvokeFunctionProperty(uint objIdx, uint propId, byte[]? data = null)
        {
            if(!_isConnected && !_isIndividual)
                throw new DeviceNotConnectedException();

            if(data == null)
                data = new byte[0];

            FunctionPropertyStateResponse response = await WaitForData<FunctionPropertyStateResponse>(new FunctionPropertyCommand(objIdx, propId, data), _currentSeqNum);
            return response;
        }

        /// <summary>
        /// Liest den Status einer Function Property aus
        /// </summary>
        /// <param name="objIdx">ObjektIndex</param>
        /// <param name="propId">PropertyId</param>
        /// <param name="data">Daten die übergeben werden sollen</param>
        /// <returns></returns>
        /// <exception cref="System.TimeoutException" />
        public async Task<FunctionPropertyStateResponse> ReadFunctionProperty(byte objIdx, byte propId, byte[] data, bool waitForResp = false)
        {
            if(!_isConnected && !_isIndividual)
                throw new DeviceNotConnectedException();

            if(data == null)
                data = new byte[0];

            throw new NotImplementedException("ReadFunctionProperty is not implemented");

            //MsgFunctionPropertyStateReq message = new MsgFunctionPropertyStateReq(objIdx, propId, data, _address);
            //message.SequenceNumber = _currentSeqNum;

            //if (waitForResp)
            //    return (MsgFunctionPropertyStateRes)await WaitForData(message);

            // TODO implement ReadFunctionProperty Message
            //await WaitForAck(message);
            return null;
        }
        #endregion


        #region Memory
        /// <summary>
        /// Schreibt Daten in den Speicher und wartet auf RÜckmeldung vom Interface
        /// </summary>
        /// <param name="address"></param>
        /// <param name="databytes"></param>
        /// <param name="verify">Falls true werden die geschriebenen Daten überprüft</param>
        /// <returns></returns>
        /// <exception cref="System.TimeoutException" />
        public async Task MemoryWrite(uint address, byte[] databytes, IProgress<float>? progress = null, bool verify = false)
        {
            if(!_isConnected && !_isIndividual)
                throw new DeviceNotConnectedException();

            List<byte> datalist = databytes.ToList();
            uint currentPosition = address;

            MaxFrameLength = 35;
            int maxCount = MaxFrameLength - 6;

            // TODO implement MemoryWriteExtended
            byte deviceControl = 0;

            if (verifyMode == VerifyMode.Unknown)
            {
                if (GetFeature("VerifyMode") == "1")
                {
                    deviceControl = await PropertyRead<byte>(0, 14);
                    bool verifyEnabled = (deviceControl & (1 << 2)) != 0;
                    verifyMode = verifyEnabled ? VerifyMode.Enabled : VerifyMode.Disabled;
                }
                else
                {
                    verifyMode = VerifyMode.NotSupported;
                }
            }

            if(verify && verifyMode == VerifyMode.NotSupported)
            {
                throw new Exceptions.NotSupportedException("VerifyMode is not supported by this device");
            }

            if (!verify && verifyMode == VerifyMode.Enabled)
            {
                deviceControl &= 0b1111_1011;
                await PropertyWrite(0, 14, new byte[] { deviceControl }, waitForResp: true);
                verifyMode = VerifyMode.Disabled;
            }
            else if (verify && verifyMode == VerifyMode.Disabled)
            {
                deviceControl |= 0b0000_0100;
                await PropertyWrite(0, 14, new byte[] { deviceControl }, waitForResp: true);
                verifyMode = VerifyMode.Enabled;
            }

            bool useExtendedMemoryWrite = maxCount > 63;
            bool firstTry = true;

            int errorCount = 0;

            while (datalist.Count != 0)
            {
                if(errorCount > 2)
                {
                    throw new Exception("Too many errors while writing memory");
                }

                List<byte> data_temp = new List<byte>();
                if (datalist.Count >= maxCount)
                {
                    data_temp.AddRange(datalist.Take(maxCount));
                    //datalist.RemoveRange(0, maxCount);
                }
                else
                {
                    data_temp.AddRange(datalist.Take(datalist.Count));
                    //datalist.RemoveRange(0, datalist.Count);
                }

                if(useExtendedMemoryWrite)
                {
                    MemoryExtendedWrite write = new MemoryExtendedWrite(address, (uint)maxCount, datalist.ToArray());
                    if (firstTry)
                    {
                        try
                        {
                            if(verifyMode == VerifyMode.Enabled)
                            {
                                MemoryExtendedWriteResponse response = await WaitForData<MemoryExtendedWriteResponse>(new MemoryExtendedWrite(currentPosition, (uint)maxCount, data_temp.ToArray()), _currentSeqNum);
                                
                            } else
                            {
                                throw new NotImplementedException("MemoryExtendedWrite without verify is not implemented");
                            }
                                
                        } catch(Exception ex)
                        {
                            // maybe the device does not support MemoryExtendedWrite, so we fall back to normal MemoryWrite
                            useExtendedMemoryWrite = false;
                        }
                    } else
                    {
                        MemoryExtendedWriteResponse response = await WaitForData<MemoryExtendedWriteResponse>(new MemoryExtendedWrite(currentPosition, (uint)maxCount, data_temp.ToArray()), _currentSeqNum);
                    }
                    firstTry = false;
                } else
                {
                    MemoryWrite write = new MemoryWrite(currentPosition, (uint)data_temp.Count, data_temp.ToArray());
                    if(verifyMode == VerifyMode.Enabled)
                    {
                        try
                        {
                            MemoryResponse response = await WaitForData<MemoryResponse>(write, _currentSeqNum);

                            if (BitConverter.ToString(response.Data) != BitConverter.ToString(data_temp.ToArray()))
                                throw new Exception("Written Memory is not requested bytes to write");
                        } catch (Exception ex)
                        {
                            Debug.WriteLine($"Error ({errorCount.ToString("D2")}): {ex.Message}");
                            errorCount++;
                            await Disconnect();
                            await Task.Delay(2000);
                            await Connect(true);
                            continue;
                        }


                    } else
                    {
                        await WaitForAck(write, _currentSeqNum);
                    }
                }

                errorCount = 0;

                if (datalist.Count >= maxCount)
                    datalist.RemoveRange(0, maxCount);
                else
                    datalist.Clear();

                currentPosition += (uint)data_temp.Count;
                if(progress != null)
                {
                    float prog = (databytes.Length - datalist.Count) / (float)databytes.Length;
                    progress.Report(prog);
                }

                await Task.Delay(300);
            }

        }




        /// <summary>
        /// Liest den Speicher des Gerätes aus.
        /// </summary>
        /// <param name="address">Start Adresse</param>
        /// <param name="length">Anzahl der Bytes die gelesen werden sollen</param>
        /// <returns>Daten aus Speicher</returns>
        public async Task<byte[]> MemoryRead(uint address, int length)
        {
            return await MemoryRead<byte[]>(address, length);
        }

        /// <summary>
        /// Liest den Speicher des Gerätes aus.
        /// </summary>
        /// <param name="address">Start Adresse</param>
        /// <param name="length">Anzahl der Bytes die gelesen werden sollen</param>
        /// <returns>Daten aus Speicher</returns>
        /// <exception cref="System.TimeoutException" />
        public async Task<T> MemoryRead<T>(uint address, int length)
        {
            if(!_isConnected && !_isIndividual)
                throw new DeviceNotConnectedException();

            List<byte> readed = new List<byte>();
            uint currentPosition = address;
            uint toRead = (uint)length;
            uint maxCount = (uint)MaxFrameLength - 3;

            // TODO implement MemoryReadExtended
            if (maxCount > 63) maxCount = 63;

            while (true)
            {
                if (length == 0) break;

                if (length > maxCount) toRead = maxCount;
                else toRead = (uint)length;

                //MsgMemoryReadReq msg = new MsgMemoryReadReq(currentPosition, toRead, _address);
                //msg.SequenceNumber = _currentSeqNum;
                // TODO implement MemoryRead Message
                //IMessageResponse resp = await WaitForData(msg);
                //readed.AddRange(resp.Raw.Skip(2));
                currentPosition += toRead;
                length -= (int)toRead;
            }

            //MsgMemoryReadRes Converter nutzen
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.String:
                    string datas = BitConverter.ToString(readed.ToArray()).Replace("-", "");
                    return (T)Convert.ChangeType(datas, typeof(T));

                case TypeCode.Int32:
                    byte[] datai = readed.ToArray();
                    byte[] xint = new byte[4];

                    for (int i = 0; i < datai.Length; i++)
                    {
                        xint[i] = datai[i];
                    }
                    return (T)Convert.ChangeType(BitConverter.ToUInt32(xint, 0), typeof(T));

                default:
                    try
                    {
                        return (T)Convert.ChangeType(readed.ToArray(), typeof(T));
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Data kann nicht in angegebenen Type konvertiert werden. " + typeof(T).ToString(), e);
                    }
            }
        }

        #endregion

        /// <summary>
        /// Autorisiert sich mit angegebenen key
        /// </summary>
        /// <param name="key">Schlüssel</param>
        /// <returns>Zugriffslevel</returns>
        public async Task<byte> Authorize(uint key)
        {
            if(!_isConnected && !_isIndividual)
                throw new DeviceNotConnectedException();

            throw new NotImplementedException("Authorize is not implemented");

            //MsgAuthorizeReq message = new MsgAuthorizeReq(key, _address);
            //message.SequenceNumber = _currentSeqNum;
            //// TODO implement Authorize Message
            //MsgAuthorizeRes resp = new MsgAuthorizeRes(); // = (MsgAuthorizeRes)await WaitForData(message);
            return 0;
        }

        /// <summary>
        /// Liest die Maskenversion des Gerätes aus
        /// </summary>
        /// <returns>Maskenversion als HexString</returns>
        /// <exception cref="System.TimeoutException" />
        public async Task<string> DeviceDescriptorRead()
        {
            if(!_isConnected && !_isIndividual)
                throw new DeviceNotConnectedException();

            DeviceDescriptorResponse response = await WaitForData< DeviceDescriptorResponse>(new DeviceDescriptorRead(), _currentSeqNum);
            MaskVersion = (ushort)(response.DescriptorData[0] << 8 | response.DescriptorData[1]);
            _mask = "MV-" + BitConverter.ToString(response.DescriptorData).Replace("-", "");
            return _mask;
        }

        public async Task<int> ReadMaxAPDULength()
        {
            ushort mediumIndependent = (ushort)((MaskVersion ?? 0) & 0x0fff);
            switch (mediumIndependent)
            {
                case 0x07B0:
                case 0x0920:
                    MaxFrameLength = await PropertyRead<int>(0, 56);
                    break;
                default:
                    throw new Exception("Unsupported MaskVersion " + mediumIndependent.ToString("X4"));
            }

            return MaxFrameLength;
        }

        public T ConvertRawData<T>(byte[] data)
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.String:
                    string datas = BitConverter.ToString(data.ToArray()).Replace("-", "");
                    return (T)Convert.ChangeType(datas, typeof(T));

                case TypeCode.Byte:
                    {
                        return (T)Convert.ChangeType(data[0], typeof(T));
                    }

                case TypeCode.Int16:
                    {
                        byte[] datai = data.Reverse().ToArray();
                        byte[] xint = new byte[2];

                        for (int i = 0; i < datai.Length; i++)
                        {
                            xint[i] = datai[i];
                        }
                        return (T)Convert.ChangeType(BitConverter.ToInt16(xint, 0), typeof(T));
                    }

                case TypeCode.UInt16:
                    {
                        byte[] datai = data.Reverse().ToArray();
                        byte[] xint = new byte[2];

                        for (int i = 0; i < datai.Length; i++)
                        {
                            xint[i] = datai[i];
                        }
                        return (T)Convert.ChangeType(BitConverter.ToUInt16(xint, 0), typeof(T));
                    }

                case TypeCode.Int32:
                    {
                        byte[] datai = data.Reverse().ToArray();
                        byte[] xint = new byte[4];

                        for (int i = 0; i < datai.Length; i++)
                        {
                            xint[i] = datai[i];
                        }
                        return (T)Convert.ChangeType(BitConverter.ToInt32(xint, 0), typeof(T));
                    }

                case TypeCode.UInt32:
                    {
                        byte[] datai = data.Reverse().ToArray();
                        byte[] xint = new byte[4];

                        for (int i = 0; i < datai.Length; i++)
                        {
                            xint[i] = datai[i];
                        }
                        return (T)Convert.ChangeType(BitConverter.ToUInt32(xint, 0), typeof(T));
                    }

                default:
                    try
                    {
                        return (T)Convert.ChangeType(data.ToArray(), typeof(T));
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Data kann nicht in angegebenen Type konvertiert werden. " + typeof(T).ToString(), e);
                    }
            }
        }

        public string GetFeature(string name)
        {
            if (features.Count == 0)
                LoadFeatures();

            if (features.ContainsKey(name))
                return features[name];
            return "";
        }

        private void LoadFeatures()
        {
            //Load Features
            features = new Dictionary<string, string>();

            XDocument master = ResourcenHelper.GetKnxMaster();
            if (master.Root == null)
                throw new Exception("Cant create Master");
            XNamespace ns = master.Root.Name.Namespace;
            XElement xmask = master.Root.Descendants(ns + "MaskVersion").Single(e => e.Attribute("Id")?.Value == _mask);
            foreach (XElement xfeature in xmask.Element(ns + "HawkConfigurationData")?.Descendants(ns + "Feature") ?? new List<XElement>())
            {
                string? name = xfeature.Attribute("Name")?.Value;
                string? value = xfeature.Attribute("Value")?.Value;

                if (name != null && value != null)
                    features[name] = value;
                else
                    throw new Exception("Feature has no Name or Value");
            }

            ManagmentModel = xmask.Attribute("ManagementModel")?.Value switch
            {
                "None" => ManagementModels.None,
                "SystemB" => ManagementModels.SystemB,
                "Bcu1" => ManagementModels.Bcu1,
                "Bcu2" => ManagementModels.Bcu2,
                "BimM112" => ManagementModels.BimM112,
                "PropertyBased" => ManagementModels.PropertyBased,
                _ => throw new Exception($"Unbekanntes ManagementModel: {xmask.Attribute("ManagementModel")?.Value ?? "null"}")
            };
        }


        private enum VerifyMode
        {
            Unknown, NotSupported, Disabled, Enabled
        }
    }
}
