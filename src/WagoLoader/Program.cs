using System;
using System.Reflection;
using WagoLoader.Network;
using WagoLoader.Wago;

namespace WagoLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("WAGO Loader v" + Assembly.GetEntryAssembly().GetName().Version);
            Console.WriteLine();

            var addr = Browser.FindIpV4Devices();
            foreach (var address in addr)
            {
                Console.Write(address + " : ");
                var di = WagoService.QueryDeviceInfo(address.ToString());
                Console.WriteLine(di != null ? di.ToString() : "no WAGO");
            }

            Console.WriteLine("done.");
            Console.ReadLine();
        }
    }
}
