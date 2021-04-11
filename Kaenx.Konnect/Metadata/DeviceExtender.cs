using Kaenx.Konnect.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Metadata
{

    public static class DeviceExtender
    {


        /// <summary>
        /// Loads the device metadata
        /// </summary>
        /// <param name="busDevice"></param>
        /// <returns></returns>
        public static async Task GetDeviceMetadata(this Kaenx.Konnect.Classes.BusDevice busDevice, RawMetaData.ReadListDefinition pReadListDefinition = RawMetaData.ReadListDefinition.Default)
        {
            RawMetaData r = await GetRawDeviceMetadata(busDevice);

            foreach (KeyValuePair<string, RawKnxDeviceMetadata> k in r.ReadList)
            {

                if (k.Value.PropertyValue != null)
                {
                    if (k.Value.PropertyValue is byte[])
                    {
                        string bytesReadable = BitConverter.ToString(k.Value.PropertyValue as Byte[]);
                        string stringRepresentation = ASCIIEncoding.ASCII.GetString(k.Value.PropertyValue as byte[]).ToLiteral();
                        Console.WriteLine(k.Key + ": " + bytesReadable + "[" + stringRepresentation + "]");
                    }
                    else
                        Console.WriteLine(k.Key + ": " + k.Value.PropertyValue.ToString());
                }
                else
                    if (k.Value.RawPropertyValue == null)
                    Console.WriteLine(k.Key + ": PropertyValue is null, " + (k.Value.ReadSuccessful ? "Read was ok" : "Read failed") + ", Raw Object is null too.");
                else
                    Console.WriteLine(k.Key + ": PropertyValue is null, " + (k.Value.ReadSuccessful ? "Read was ok" : "Read failed") + ", Raw Object contains " + k.Value.RawPropertyValue.Length + " Bytes");
            }
        }

        /// <summary>
        /// Loads the device metadata
        /// </summary>
        /// <param name="busDevice"></param>
        /// <returns></returns>
        public static async Task GetDeviceMetadata(this Kaenx.Konnect.Classes.BusDevice busDevice, Dictionary<string, RawKnxDeviceMetadata> pReadlistB)
        {
           RawMetaData r =  await GetRawDeviceMetadata(busDevice);

            foreach (KeyValuePair<string, RawKnxDeviceMetadata> k in r.ReadList)
            {             

                if (k.Value.PropertyValue != null)
                {
                    if (k.Value.PropertyValue is byte[])
                    {
                        string bytesReadable = BitConverter.ToString(k.Value.PropertyValue as Byte[]);
                        string stringRepresentation = ASCIIEncoding.ASCII.GetString(k.Value.PropertyValue as byte[]).ToLiteral();
                        Console.WriteLine(k.Key + ": " + bytesReadable  + "[" + stringRepresentation  + "]");
                    }
                    else
                        Console.WriteLine(k.Key + ": " + k.Value.PropertyValue.ToString());
                }
                else
                    if (k.Value.RawPropertyValue == null)
                        Console.WriteLine(k.Key + ": PropertyValue is null, " + (k.Value.ReadSuccessful ? "Read was ok" : "Read failed") + ", Raw Object is null too.");
                else
                        Console.WriteLine(k.Key + ": PropertyValue is null, " + (k.Value.ReadSuccessful ? "Read was ok" : "Read failed") + ", Raw Object contains " + k.Value.RawPropertyValue.Length + " Bytes");
            }
        }

        /// <summary>
        /// Basic ping function the check if the device is responding
        /// </summary>
        /// <param name="busDevice"></param>
        /// <returns></returns>
       public static async Task<bool> PingDevice(BusDevice busDevice)
        {
            try
            {
                Byte[] ba = await busDevice.PropertyRead(0x00, 0x01);
                UInt16 ui = BitConverter.ToUInt16(ba, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Read the device metadata
        /// </summary>
        /// <param name="busDevice"></param>
        /// <param name="throttling"></param>
        /// <returns></returns>
        private static async Task<RawMetaData> GetRawDeviceMetadata(BusDevice busDevice, int throttling = 300, RawMetaData.ReadListDefinition pDefinition=RawMetaData.ReadListDefinition.Default)
        {
            // Sanity Checks
            // Read 0x00 0x01, we are expecting a zero here per KNX Standard
            Byte[] ba = await busDevice.PropertyRead(0x00, 0x01);
            UInt16 ui = BitConverter.ToUInt16(ba,0);

            Byte[] ba2 = await busDevice.PropertyRead(0x00, 0x02);
            string s = ASCIIEncoding.ASCII.GetString(ba2);



            RawMetaData rawData = new RawMetaData(pDefinition);
            foreach (RawKnxDeviceMetadata kvp in rawData.ReadList.Values)
            {
                Thread.Sleep(throttling);
                kvp.TriedReading = true;
                try
                {
                    Byte[] baValue = await busDevice.PropertyRead<byte[]>((Byte)kvp.ObjectId, (Byte)kvp.ValueId);

                    kvp.ReadSuccessful = true;
                    kvp.RawPropertyValue = baValue;
                }
                catch(Exception Ex)
                {
                    Console.WriteLine(Ex.ToString());
                    kvp.ReadSuccessful = false;
                }
            }

            return rawData;
        }


        private static async Task<RawMetaData> GetRawDeviceMetadata(BusDevice busDevice, Dictionary<string, RawKnxDeviceMetadata> pReadlist, int throttling = 300)
        {
            // Sanity Checks
            // Read 0x00 0x01, we are expecting a zero here per KNX Standard
            Byte[] ba = await busDevice.PropertyRead(0x00, 0x01);
            UInt16 ui = BitConverter.ToUInt16(ba, 0);

            Byte[] ba2 = await busDevice.PropertyRead(0x00, 0x02);
            string s = ASCIIEncoding.ASCII.GetString(ba2);



            RawMetaData rawData = new RawMetaData(RawMetaData.ReadListDefinition.Empty);
            rawData.ReadList = pReadlist;

            foreach (RawKnxDeviceMetadata kvp in rawData.ReadList.Values)
            {
                Thread.Sleep(throttling);
                kvp.TriedReading = true;
                try
                {
                    Byte[] baValue = await busDevice.PropertyRead<byte[]>((Byte)kvp.ObjectId, (Byte)kvp.ValueId);

                    kvp.ReadSuccessful = true;
                    kvp.RawPropertyValue = baValue;
                }
                catch (Exception Ex)
                {
                    Console.WriteLine(Ex.ToString());
                    kvp.ReadSuccessful = false;
                }
            }

            return rawData;
        }

        // Helper function for a readable debugging output
        public static string ToLiteral(this string input)
        {
            StringBuilder literal = new StringBuilder(input.Length + 2);
            literal.Append("\"");
            foreach (var c in input)
            {
                switch (c)
                {
                    case '\'': literal.Append(@"\'"); break;
                    case '\"': literal.Append("\\\""); break;
                    case '\\': literal.Append(@"\\"); break;
                    case '\0': literal.Append(@"\0"); break;
                    case '\a': literal.Append(@"\a"); break;
                    case '\b': literal.Append(@"\b"); break;
                    case '\f': literal.Append(@"\f"); break;
                    case '\n': literal.Append(@"\n"); break;
                    case '\r': literal.Append(@"\r"); break;
                    case '\t': literal.Append(@"\t"); break;
                    case '\v': literal.Append(@"\v"); break;
                    default:
                        // ASCII printable character
                        if (c >= 0x20 && c <= 0x7e)
                        {
                            literal.Append(c);
                            // As UTF16 escaped character
                        }
                        else
                        {
                            literal.Append(@"\u");
                            literal.Append(((int)c).ToString("x4"));
                        }
                        break;
                }
            }
            literal.Append("\"");
            return literal.ToString();
        }
    }
}
