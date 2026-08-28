using Kaenx.Konnect.Classes.Helper;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kaenx.Konnect.Classes
{
    public class DeviceEnumerator  : IUsbDeviceEnumerator
    {
        // ---------------------------------------------------------
        // LOAD knx_interfaces.xml  (CORRECT FOR YOUR XML)
        // ---------------------------------------------------------
        private static Dictionary<(string vid, string pid), string> LoadInterfaceDefinitions()
        {
            XDocument doc = ResourcenHelper.GetKnxInterfaces();

            return doc.Root
                .Elements("Interface")
                .ToDictionary(
                    x => (
                        ((string)x.Attribute("VendorID") ?? "").ToUpper(),
                        ((string)x.Attribute("ProductID") ?? "").ToUpper()
                    ),
                    x =>
                    {
                        // Prefer translation in system language
                        var translation = x.Elements("Translation")
                            .FirstOrDefault(t => (string)t.Attribute("Language") == "de-DE");

                        if (translation != null)
                            return (string)translation.Attribute("Text");

                        // Fallback: DefaultDisplayText
                        return (string)x.Attribute("DefaultDisplayText") ?? "Unknown USB Interface";
                    }
                );
        }

        // ---------------------------------------------------------
        // FT1.2 HANDSHAKE
        // ---------------------------------------------------------
        private static bool TryKnxHandshake(string port, out byte[] response)
        {
            response = Array.Empty<byte>();

            try
            {
                using var sp = new SerialPort(port, 19200, Parity.Even, 8, StopBits.One)
                {
                    ReadTimeout = 150,
                    WriteTimeout = 150
                };

                sp.Open();

                // Standard FT1.2 init frame
                byte[] init = { 0x10, 0x81, 0x10 };
                sp.Write(init, 0, init.Length);

                Thread.Sleep(50);

                if (sp.BytesToRead > 0)
                {
                    response = new byte[sp.BytesToRead];
                    sp.Read(response, 0, response.Length);
                    return true;
                }
            }
            catch
            {
                // Port nicht nutzbar → ignorieren
            }

            return false;
        }

        // ---------------------------------------------------------
        // PARSE VENDOR/PRODUCT FROM FT1.2 FRAME
        // ---------------------------------------------------------
        private static (string vid, string pid) ParseVendorProduct(byte[] frame)
        {
            // Minimaler FT1.2-Frame: 0x68 LL LL 0x68 0x53 <VID> <PID> ... CHK 0x16
            if (frame.Length < 10)
                return ("", "");

            string vid = frame[6].ToString("X2");
            string pid = frame[7].ToString("X2");

            return (vid, pid);
        }

        public IEnumerable<KnxUsbDeviceInfo> GetKnxDevices()
        {
            var interfaces = LoadInterfaceDefinitions();

            foreach (var port in SerialPort.GetPortNames())
            {
                if (!TryKnxHandshake(port, out var response))
                    continue;

                var (vid, pid) = ParseVendorProduct(response);

                if (interfaces.TryGetValue((vid, pid), out var name))
                {
                    yield return new KnxUsbDeviceInfo
                    {
                        PortName = port,
                        VendorId = vid,
                        ProductId = pid,
                        Name = name
                    };
                }
            }

#if DEBUG
            const string vid_debug = "0908";
            const string pid_debug = "02DD";
            //if (interfaces.TryGetValue((vid_debug, pid_debug), out var namedebug))
            //{
            //    yield return new KnxUsbDeviceInfo
            //    {
            //        PortName = "COM01",
            //        VendorId = vid_debug,
            //        ProductId = pid_debug,
            //        Name = namedebug
            //    };
            //}
#endif
        }

        public async Task<bool> RequestPermissionAsync(KnxUsbDeviceInfo device)
        {
            return await Task.FromResult(true);
        }
    }

    public class KnxUsbDeviceInfo
    {

        /// <summary>Anzeigename aus knx_interfaces.xml oder UsbDevice.ProductName.</summary>
        public string Name { get; set; } = "Unknown KNX USB Interface";

        /// <summary>z.B. "0E77"</summary>
        public string VendorId { get; set; } = "";

        /// <summary>z.B. "0111"</summary>
        public string ProductId { get; set; } = "";

        /// <summary>Herstellername aus UsbDevice, falls vorhanden.</summary>
        public string? Manufacturer { get; set; }

        /// <summary>Seriennummer aus UsbDevice, falls vorhanden.</summary>
        public string? PortName { get; set; }

        /// <summary>
        ///   True sobald UsbManager.HasPermission() positiv ist
        ///   oder Permission explizit gewährt wurde.
        /// </summary>
        public bool HasPermission { get; set; }

        public string? SerialNumber { get; set; }
        public int DeviceId { get; set; }

        public override string ToString() =>
            $"{Name} (VID={VendorId} PID={ProductId})";
    }


    // ============================================================
    // Services/IUsbDeviceEnumerator.cs
    // ============================================================

    public interface IUsbDeviceEnumerator
    {
        /// <summary>
        ///   Gibt alle aktuell angeschlossenen KNX-USB-Interfaces zurück.
        ///   Wird synchron aufgerufen (schnell, kein I/O).
        /// </summary>
        IEnumerable<KnxUsbDeviceInfo> GetKnxDevices();

        /// <summary>
        ///   Fordert USB-Permission für das Gerät an.
        ///   Gibt true zurück wenn der User gewährt hat.
        /// </summary>
        Task<bool> RequestPermissionAsync(KnxUsbDeviceInfo device);
    }
}
