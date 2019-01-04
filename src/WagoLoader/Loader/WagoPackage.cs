using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json;

namespace WagoLoader.Loader
{
    public class WagoPackage
    {
        public const string SpecFileName = "wagopackage.json";

        public string PackageName { get; }

        public PackageSpec Specification { get; private set; }

        private WagoPackage(string packageName)
        {
            PackageName = packageName;
        }

        public static WagoPackage Load(string packageName)
        {
            var package = new WagoPackage(packageName);

            try
            {
                using (var zip = ZipFile.OpenRead(package.PackageName))
                {
                    var spec = zip.GetEntry(SpecFileName);
                    if (spec == null)
                    {
                        Console.WriteLine("ERROR: Package does not contain specification file.");
                        return null;
                    }

                    var json = new StreamReader(spec.Open()).ReadToEnd();
                    package.Specification = JsonConvert.DeserializeObject<PackageSpec>(json);
                    return package;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Failed to load package specification: " + ex.Message);
                package.Specification = null;
            }

            return null;
        }

        public List<string> GetPackageFiles()
        {
            var files = new List<string>();
            try
            {
                using (var zip = ZipFile.OpenRead(PackageName))
                {
                    files = zip.Entries
                        .Select(ent => ent.FullName)
                        .ToList();
                }
            }
            catch (Exception)
            {
                // ignore
            }

            return files;
        }

        public bool ExtractFile(string name, string targetPath)
        {
            try
            {
                using (var zip = ZipFile.OpenRead(PackageName))
                {
                    var spec = zip.GetEntry(name);
                    if (spec == null)
                    {
                        Console.WriteLine("ERROR: Package does not contain file " + name);
                        return false;
                    }

                    using (var wrt = new StreamWriter(targetPath))
                    {
                        spec.Open().CopyTo(wrt.BaseStream);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed extract file {name}: " + ex.Message);
            }

            return false;
        }

    }
}
