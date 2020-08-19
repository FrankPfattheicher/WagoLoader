using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using WagoLoader.Loader;

namespace WagoLoader.Commands
{
    internal class CreatePackage
    {
        private static CommandArgument _srcPath;
        private static CommandArgument _package;
        private static CommandOption _dstPath;

        internal static void Create(CommandLineApplication command)
        {
            command.Description = "Create deployment package";

            _srcPath = command.Argument(
                "source path",
                "The source path of the project");
            _package = command.Argument(
                "package",
                "The package name to be created (default is the project name)");

            _dstPath = command.Option("-o", "output path", CommandOptionType.SingleValue);
            _dstPath.LongName = "output-path";

            command.OnExecute(() => Execute());
        }

        internal static int Execute()
        {
            var sourcePath = _srcPath.Value;
            if (string.IsNullOrEmpty(sourcePath))
            {
                sourcePath = ".";
            }

            if (!Directory.Exists(sourcePath))
            {
                Console.WriteLine($"ERROR: Source path to create package not found: {sourcePath}");
                return 1;
            }
            var destPath = sourcePath;
            if(!string.IsNullOrEmpty(_dstPath.Value()))
            {
                destPath = _dstPath.Value();
                if (!Directory.Exists(destPath))
                {
                    Console.WriteLine($"ERROR: Output path to create package not found: {destPath}");
                    return 1;
                }
            }

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

            packageName = Path.GetFileNameWithoutExtension(packageName) + ".wago";
            packageName = Path.Combine(destPath, packageName);

            var packageSpec = Path.Combine(sourcePath, WagoPackage.SpecFileName);
            if (!File.Exists(packageSpec))
            {
                Console.WriteLine($"ERROR: No package specification file '{WagoPackage.SpecFileName}' found in {sourcePath}.");
                return 1;
            }
            Console.WriteLine($"package specification file {packageSpec}");
            var json = File.ReadAllText(packageSpec);
            var package = JsonConvert.DeserializeObject<PackageSpec>(json);
            var products = package.System.Products;
            
            var projectFiles = new List<string>();
            foreach (var product in products)
            {
                var name = Path.GetFileNameWithoutExtension(product.PackageName);

                var programName = Directory.EnumerateFiles(sourcePath, $"{name}.prg").FirstOrDefault();
                if (programName == null)
                {
                    Console.WriteLine($"ERROR: No project file ({name}.prg) found in source path {sourcePath}.");
                    return 1;
                }
                var checkName = Directory.EnumerateFiles(sourcePath, $"{name}.chk").FirstOrDefault();
                if (checkName == null)
                {
                    Console.WriteLine($"ERROR: No checksum file ({name}.chk) found in source path {sourcePath}.");
                    return 1;
                }
                projectFiles.Add(programName);
            }

            Console.WriteLine();

            Console.WriteLine($"Creating {Path.GetFileName(packageName)} from path {sourcePath} ...");

            if (File.Exists(packageName))
            {
                File.Delete(packageName);
            }

            using (var zip = ZipFile.Open(packageName, ZipArchiveMode.Create))
            {
                // add package spec file
                zip.CreateEntryFromFile(packageSpec, WagoPackage.SpecFileName);

                // add project files
                foreach (var projectFile in projectFiles)
                {
                    zip.CreateEntryFromFile(projectFile, Path.GetFileName(projectFile));
                    var checkFile = Path.ChangeExtension(projectFile, "chk");
                    zip.CreateEntryFromFile(checkFile, Path.GetFileName(checkFile));
                }

                // add content files
                var contentDir = Path.Combine(sourcePath, "FileSystem");
                if (Directory.Exists(contentDir))
                {
                    var hidden = Path.DirectorySeparatorChar + ".";
                    var contentFiles = Directory
                        .EnumerateFiles(contentDir, "*.*", SearchOption.AllDirectories)
                        .Where(fn => !fn.Contains(hidden))
                        .ToArray();
                    foreach (var contentFile in contentFiles)
                    {
                        var contentName = "filesystem" + contentFile.Substring(contentDir.Length);
                        zip.CreateEntryFromFile(contentFile, contentName);
                    }
                }

            }

            Console.WriteLine("Package created successfully.");
            return 0;
        }

    }
}
