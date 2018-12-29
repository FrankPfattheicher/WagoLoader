using System;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;

namespace WagoLoader.Loader
{
    public class WagoPackage
    {
        public const string SpecFileName = "wagopackage.json";

        private readonly string _packageName;
        
        public PackageSpec Specification { get; private set; }

        public WagoPackage(string packageName)
        {
            _packageName = packageName;
        }
        
        public bool LoadPackage()
        {
            try
            {
                using (var zip = ZipFile.OpenRead(_packageName))
                {
                    var spec = zip.GetEntry(SpecFileName);
                    if (spec == null)
                    {
                        Console.WriteLine("ERROR: Package does not contain specification file.");
                        return false;
                    }

                    var json = new StreamReader(spec.Open()).ReadToEnd();
                    Specification = JsonConvert.DeserializeObject<PackageSpec>(json);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Failed dot load package specification: " + ex.Message);
                Specification = null;
            }

            return false;
        }



    }
}
