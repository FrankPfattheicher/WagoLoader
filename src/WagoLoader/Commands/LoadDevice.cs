using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using Renci.SshNet;
using WagoLoader.Loader;
using WagoLoader.Wago;
// ReSharper disable StringLiteralTypo

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

            var package = WagoPackage.Load(packageName);
            if (package == null)
            {
                return 1;
            }

            if (!package.Specification.System.Product.Contains(di.ProductSerialNumber))
            {
                Console.WriteLine($"ERROR: The target controller type ({di.ProductSerialNumber}) does not match the package specification.");
                return 1;
            }

            Console.WriteLine();
            Console.WriteLine($"Installing package {Path.GetFileNameWithoutExtension(packageName)}");
            Console.WriteLine($"    {package.Specification.Description} - version {package.Specification.Version}");
            Console.WriteLine($"    on WAGO controller {di.ProductSerialNumber}");
            Console.WriteLine();

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

            // set timezone
            var timezone = package.Specification.System.Timezone;
            if (!string.IsNullOrEmpty(timezone))
            {
                Console.WriteLine("Setting time zone to " + timezone);
                Timezone.SetTimezone(shell, rootPwd, timezone);
            }

            // set WBM users as given in the packet
            Console.WriteLine("Loading WBM users...");
            const string pwdFileName = "lighttpd-htpasswd.user";
            try
            {
                File.WriteAllText(pwdFileName, "");
                var pwdFile = new PasswordFile(pwdFileName);
                foreach (var wbmUser in package.Specification.Users.Wbm)
                {
                    pwdFile.SetPassword(wbmUser.Name, wbmUser.Password);
                }

                var inf = new FileInfo(pwdFileName);
                using (var scp = new ScpClient(_controller.Value, "root", rootPwd))
                {
                    scp.RemotePathTransformation = RemotePathTransformation.ShellQuote;
                    scp.Connect();
                    if (!scp.IsConnected)
                    {
                        Console.WriteLine("ERROR: Could connect upload SCP.");
                        return 1;
                    }
                    scp.Upload(inf, $"/etc/lighttpd/{pwdFileName}");
                }
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
            var tmpPrg = Path.GetTempFileName();
            var tmpChk = Path.GetTempFileName();
            try
            {
                Console.WriteLine("Loading Codesys project...");

                using (var scp = new ScpClient(_controller.Value, "root", rootPwd))
                {
                    scp.RemotePathTransformation = RemotePathTransformation.ShellQuote;
                    scp.Connect();
                    if (!scp.IsConnected)
                    {
                        Console.WriteLine("ERROR: Could connect upload project.");
                        return 1;
                    }

                    package.ExtractFile("DEFAULT.PRG", tmpPrg);
                    var prg = new FileInfo(tmpPrg);
                    scp.Upload(prg, "/home/codesys/DEFAULT.PRG");

                    package.ExtractFile("DEFAULT.CHK", tmpChk);
                    var chk = new FileInfo(tmpChk);
                    scp.Upload(chk, "/home/codesys/DEFAULT.CHK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Failed to upload project files: " + ex.Message);
                return 1;
            }
            finally
            {
                if (File.Exists(tmpPrg)) File.Delete(tmpPrg);
                if (File.Exists(tmpChk)) File.Delete(tmpChk);
            }


            // transfer file system files
            var contentFiles = package.GetPackageFiles()
                .Where(fn => fn.StartsWith("filesystem", StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            if (contentFiles.Count > 0)
            {
                Console.WriteLine("Creating file system content directories...");
                var contentDirectories = contentFiles
                    .Select(cf => Path.GetDirectoryName(cf.Substring("filesystem".Length)).Replace(Path.DirectorySeparatorChar, '/'))
                    .Distinct()
                    .ToList();
                foreach (var contentDirectory in contentDirectories)
                {
                    shell.ExecCommand("root", rootPwd, $"mkdir {contentDirectory}");
                }

                Console.WriteLine($"Loading {contentFiles.Count} file system content files...");
                foreach (var contentFile in contentFiles)
                {
                    var tmpFile = Path.GetTempFileName();
                    try
                    {
                        using (var scp = new ScpClient(_controller.Value, "root", rootPwd))
                        {
                            scp.RemotePathTransformation = RemotePathTransformation.ShellQuote;
                            scp.Connect();
                            if (!scp.IsConnected)
                            {
                                Console.WriteLine();
                                Console.WriteLine("ERROR: Could connect upload content.");
                                return 1;
                            }

                            package.ExtractFile(contentFile, tmpFile);
                            var sourceFile = new FileInfo(tmpFile);
                            var targetFileName = contentFile.Substring("filesystem".Length).Replace(Path.DirectorySeparatorChar, '/');

                            Console.Write(".");
                            scp.Upload(sourceFile, targetFileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"ERROR: Failed to upload content file {contentFile}: " + ex.Message);
                        return 1;
                    }
                    finally
                    {
                        if (File.Exists(tmpFile)) File.Delete(tmpFile);
                    }

                }
            }
            Console.WriteLine();

            Console.WriteLine("Done.");
            WagoService.ResetDevice(_controller.Value);
            Console.WriteLine("Restarting controller.");

            return 0;
        }
    }
}
