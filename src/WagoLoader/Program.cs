using System;
using System.Net.Sockets;
using System.Text;

namespace WagoLoader
{
    class Program
    {
        private static string ipAddress = "192.168.2.165";

        private static int rqDeviceInfoPort = 6626;

        private static byte[] rqDeviceInfo = new byte[] {
    0x88, 0x12, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x08, 0x01
};

        static void Main(string[] args)
        {
            Console.WriteLine("WAGO Loader");


            using (var client = new TcpClient())
            {
                client.Connect(ipAddress, rqDeviceInfoPort);
                var stream = client.GetStream();
                stream.ReadTimeout = 1000;
                stream.Write(rqDeviceInfo, 0, rqDeviceInfo.Length);

                var rxBuffer = new byte[4096];
                var rxLength = stream.Read(rxBuffer, 0, rxBuffer.Length);

                var text = Encoding.ASCII.GetString(rxBuffer, 28, rxLength - 29);
                text = text.Replace(";", Environment.NewLine);
                Console.WriteLine(text);
            }

            Console.ReadLine();
        }
    }
}
