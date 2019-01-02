﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using Renci.SshNet;
using WagoLoader.Loader;
using WagoLoader.Wago;

namespace WagoLoader.Commands
{
    internal class LoadDevice
    {
        private static CommandArgument _controller;
        private static CommandArgument _package;

        internal static void Create(CommandLineApplication command)
        {
            command.Description = "Load specified WAGO controller";

            _controller = command.Argument(
                "controller",
                "The ip address of the controller");
            _package = command.Argument(
                "package",
                "The package file name to load");

            command.OnExecute(() => Execute());
        }

        internal static int Execute()
        {
            if (string.IsNullOrEmpty(_controller.Value))
            {
                Console.WriteLine("ERROR: A controller address has to be specified.");
                return 1;
            }

            var packageName = _package.Value;
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

            var ext = Path.GetExtension(packageName);
            if (string.IsNullOrEmpty(ext))
            {
                packageName += ".wago";
            }

            Console.WriteLine();

            Console.WriteLine($"Loading {Path.GetFileName(packageName)} to {_controller.Value} ...");

            var di = WagoService.QueryDeviceInfo(_controller.Value);
            if (di == null)
            {
                Console.WriteLine("ERROR: At the specified address is no WAGO controller.");
                return 1;
            }

            var package = new WagoPackage(packageName);
            if (!package.LoadPackage())
            {
                return 1;
            }

            if (!package.Specification.System.Product.Contains(di.ProductSerialNumber))
            {
                Console.WriteLine($"ERROR: The target controller type ({di.ProductSerialNumber}) does not match the package specification.");
                return 1;
            }

            Console.WriteLine($"Installing package {Path.GetFileNameWithoutExtension(packageName)} on WAGO controller {di.ProductSerialNumber}");

            var shell = new RemoteShell(_controller.Value);

            // get root password from package - if already applied
            var specRootPwd = package.Specification.Users.Linux.FirstOrDefault(up => up.Name == "root");
            var rootPwd = specRootPwd?.Password ?? "";

            // detect current root password from the list to test
            rootPwd = shell.GetRootPassword(new List<string> { "wago", rootPwd });
            if (rootPwd == null)
            {
                Console.WriteLine("ERROR: Root password not matching.");
                Console.WriteLine();
                Console.WriteLine("Try factory resetting the controller.");
                return 1;
            }

            // set linux users as given in the packet
            foreach (var linuxUser in package.Specification.Users.Linux)
            {
                Console.WriteLine(shell.ChangePassword(rootPwd, linuxUser.Name, linuxUser.Password)
                    ? $"Changed password for linux user {linuxUser.Name}."
                    : $"ERROR: Failed to change password for linux user {linuxUser.Name}.");

                if (linuxUser.Name == "root")
                {
                    // save new root password
                    rootPwd = linuxUser.Password;
                }
            }

            // set WBM users as given in the packet
            const string pwdFileName = "lighttpd-htpasswd.user";
            try
            {
                File.WriteAllText(pwdFileName, "");
                var pwdFile = new Passwords(pwdFileName);
                foreach (var wbmUser in package.Specification.Users.Wbm)
                {
                    pwdFile.SetPassword(wbmUser.Name, wbmUser.Password);
                }

                var inf = new FileInfo(pwdFileName);
                var scp = new ScpClient(_controller.Value, "root", rootPwd);
                scp.Connect();
                if (!scp.IsConnected)
                {
                    Console.WriteLine("ERROR: Could connect upload SCP.");
                    return 1;
                }
                scp.Upload(inf, $"/etc/lighttpd/{pwdFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Failed to upload WBM passwords: " + ex.Message);
                return 1;
            }
            finally
            {
                File.Delete(pwdFileName);
            }

            // transfer CodeSys project
            //TODO

            // transfer file system files
            //TODO

            Console.WriteLine("done.");
            return 0;
        }
    }
}
