using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using WagoLoader.Loader;
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
            app.Command("query", QueryDevice);
            app.Command("reset", ResetDevice);
            app.Command("load", LoadDevice);

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

        private static void QueryDevice(CommandLineApplication command)
        {
            command.Description = "Query system info of the specified WAGO controller";

            var controller = command.Argument(
                "controller",
                "The ip address of the controller");

            command.OnExecute(() =>
            {
                if (string.IsNullOrEmpty(controller.Value))
                {
                    Console.WriteLine("ERROR: A controller address has to be specified.");
                    return 1;
                }
                Console.WriteLine($"Querying {controller.Value}...");
                Console.WriteLine();
                var di = WagoService.QueryDeviceInfo(controller.Value);
                if (di != null)
                {
                    Console.WriteLine($"Order number:            {di.OrderNumber}");
                    Console.WriteLine($"Description:             {di.Description}");
                    Console.WriteLine($"Serial number:           {di.SerialNumber}");
                    Console.WriteLine($"Software version:        {di.SoftwareVersion}");
                    Console.WriteLine($"Hardware version:        {di.HardwareVersion}");
                    Console.WriteLine($"Firmware loader version: {di.FirmwareLoaderVersion}");
                    Console.WriteLine($"Baud rate:               {di.BaudRate}");
                    Console.WriteLine($"Firmware burn date:      {di.FirmwareBurnDate}");
                    Console.WriteLine($"Product serial number:   {di.ProductSerialNumber}");
                    Console.WriteLine($"QS string:               {di.QsString}");
                }
                else
                {
                    Console.WriteLine("no WAGO controller");
                }
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
                    Console.WriteLine("ERROR: A controller address has to be specified.");
                    return 1;
                }
                Console.WriteLine($"Resetting {controller.Value}...");
                WagoService.ResetDevice(controller.Value);
                Console.WriteLine("done.");
                return 0;
            });
        }

        private static void LoadDevice(CommandLineApplication command)
        {
            command.Description = "Load specified WAGO controller";

            var controller = command.Argument(
                "controller",
                "The ip address of the controller");
            var package = command.Argument(
                "package",
                "The package file name to load");

            command.OnExecute(() =>
            {
                if (string.IsNullOrEmpty(controller.Value))
                {
                    Console.WriteLine("ERROR: A controller address has to be specified.");
                    return 1;
                }

                var packageName = package.Value;
                if (string.IsNullOrEmpty(packageName))
                {
                    var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    packageName = Directory.EnumerateFiles(path, "*.wago").FirstOrDefault();
                    if (string.IsNullOrEmpty(packageName))
                    {
                        Console.WriteLine("ERROR: No package specified or found locally.");
                        return 1;
                    }

                    Console.WriteLine($"Using package {Path.GetFileName(packageName)}");
                }

                Console.WriteLine();

                Console.WriteLine($"Loading {Path.GetFileName(packageName)} to {controller.Value}...");

                var di = WagoService.QueryDeviceInfo(controller.Value);
                if (di == null)
                {
                    Console.WriteLine("ERROR: At the specified address is no WAGO controller.");
                    return 1;
                }

                var pl = new PackageLoader(packageName, controller.Value);
                if (!pl.LoadPackage())
                {

                }

                var shell = new RemoteShell(controller.Value);
                var rootPwd = shell.GetRootPassword(new List<string> { "wago", "test" });
                if (rootPwd == null)
                {
                    Console.WriteLine("ERROR: Root password not matching.");
                    Console.WriteLine();
                    Console.WriteLine("Try factory resetting the controller.");
                    return 1;
                }


                Console.WriteLine("done.");
                return 0;
            });
        }

    }
}
