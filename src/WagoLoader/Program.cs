using System;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using WagoLoader.Network;
using WagoLoader.Wago;

namespace WagoLoader
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("WAGO Loader v" + Assembly.GetEntryAssembly().GetName().Version);
            Console.WriteLine();

            var app = new CommandLineApplication
            {
                Name = "WagoLoader",
                Description = "Tool to prepare new WAGO controller with all necessary settings"
            };
            app.HelpOption("-?|-h|--help");
            app.Command("scan", ScanDevices);
            app.Command("reset", ResetDevice);

            if (args.Length < 1)
            {
                app.ShowHelp();
            }
            else
            {
                app.Execute(args);
            }
        }

        private static void ScanDevices(CommandLineApplication command)
        {
            command.Description = "Scan local IPv4 network for WAGO controllers";

            command.OnExecute(() =>
            {
                Console.WriteLine("Scanning for local IPv4 devices...");

                var deviceAddresses = Browser.FindIpV4Devices();
                foreach (var address in deviceAddresses)
                {
                    Console.Write(address + "\t: ");
                    var di = WagoService.QueryDeviceInfo(address.ToString());
                    Console.WriteLine(di != null ? di.ToString() : "no WAGO controller");
                }

                Console.WriteLine("done.");
                return 0;
            });
        }

        private static void ResetDevice(CommandLineApplication command)
        {
            command.Description = "Reset specified WAGO controller";

            var controller = command.Argument(
                        "controller",
                        "The ip address of the controller");

            command.OnExecute(() =>
            {
                if (string.IsNullOrEmpty(controller.Value))
                {
                    Console.WriteLine($"ERROR: A controller address has to be specified.");
                    return 1;
                }
                Console.WriteLine($"Resetting {controller.Value}...");
                WagoService.ResetDevice(controller.Value);
                Console.WriteLine("done.");
                return 0;
            });
        }

    }
}
