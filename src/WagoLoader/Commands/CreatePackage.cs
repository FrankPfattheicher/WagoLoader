using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using WagoLoader.Loader;

namespace WagoLoader.Commands
{
    internal class CreatePackage
    {
        private static CommandArgument _srcPath;
        private static CommandArgument _package;

        internal static void Create(CommandLineApplication command)
        {
            command.Description = "Create deployment package";

            _srcPath = command.Argument(
                "source path",
                "The source path of the project");
            _package = command.Argument(
                "package",
                "The package name to be created (default is the project name)");

            command.OnExecute(() => Execute());
        }

        internal static int Execute()
        {
            var sourcePath = _srcPath.Value;
            if (string.IsNullOrEmpty(sourcePath))
            {
                sourcePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }

            if (!Directory.Exists(sourcePath))
            {
                Console.WriteLine($"ERROR: Source path to create package not found: {sourcePath}");
                return 1;
            }
            var destPath = sourcePath;

            var packageName = _package.Value;
            if (string.IsNullOrEmpty(packageName))
            {
                packageName = Directory.EnumerateFiles(sourcePath, "*.pro").FirstOrDefault();
                if (string.IsNullOrEmpty(packageName))
                {
                    Console.WriteLine($"ERROR: No package specified and no project found in {sourcePath}.");
                    return 1;
                }
                Console.WriteLine($"Using project file {Path.GetFileName(packageName)}");
            }
            else
            {
                var packageDir = Path.GetDirectoryName(packageName);
                if (string.IsNullOrEmpty(packageDir))
                {
                    packageName = Path.Combine(destPath, packageName);
                }
            }

            var ext = Path.GetExtension(packageName);
            if (string.IsNullOrEmpty(ext))
            {
                packageName += ".wago";
            }

            var packageSpec = Path.Combine(sourcePath, WagoPackage.SpecFileName);
            if (!File.Exists(packageSpec))
            {
                Console.WriteLine($"ERROR: No package specification file '{WagoPackage.SpecFileName}' found in {sourcePath}.");
                return 1;
            }

            Console.WriteLine();

            Console.WriteLine($"Creating {Path.GetFileName(packageName)} from path {sourcePath} ...");

            if (File.Exists(packageName))
            {
                File.Delete(packageName);
            }

            using (var zip = ZipFile.Open(packageName, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(packageSpec, WagoPackage.SpecFileName);
            }

            Console.WriteLine("Package created successfully.");
            return 0;
        }

    }
}
