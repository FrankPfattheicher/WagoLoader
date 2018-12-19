namespace WagoLoader.Loader
{
    public class PackageLoader
    {
        private readonly string _packageName;
        private readonly string _controllerAddress;

        public PackageLoader(string packageName, string controllerAddress)
        {
            _packageName = packageName;
            _controllerAddress = controllerAddress;
        }
        
        public bool LoadPackage()
        {
            return true;
        }



    }
}
