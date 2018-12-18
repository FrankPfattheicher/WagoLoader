using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace WagoLoader.Wago
{
    public class WagoService
    {
        public static int PortNumber = 6626;

        public static DeviceInfo QueryDeviceInfo(string ipAddress)
        {
            // no further information available - maybe someone has?
            var rqDeviceInfo = new byte[] {
                    0x88, 0x12, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x08, 0x01
                };

            try
            {
                using (var client = new TcpClient())
                {
                    client.Connect(ipAddress, PortNumber);
                    var stream = client.GetStream();
                    stream.ReadTimeout = 1000;
                    stream.Write(rqDeviceInfo, 0, rqDeviceInfo.Length);

                    var rxBuffer = new byte[4096];
                    var rxLength = stream.Read(rxBuffer, 0, rxBuffer.Length);

                    var text = Encoding.ASCII.GetString(rxBuffer, 28, rxLength - 29);
                    return DeviceInfo.Parse(text);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }

            return null;
        }

        public static void ResetDevice(string ipAddress)
        {
            var rqDeviceReset = new byte[] {
                0x88, 0x12, 0x30, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x02, 0x01, 0x00, 0x00, 0x2d, 0x00
            };

            try
            {
                using (var client = new TcpClient())
                {
                    client.Connect(ipAddress, PortNumber);
                    var stream = client.GetStream();
                    stream.ReadTimeout = 1000;
                    stream.Write(rqDeviceReset, 0, rqDeviceReset.Length);

                    var rxBuffer = new byte[4096];
                    stream.Read(rxBuffer, 0, rxBuffer.Length);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

    }
}
