using System.Collections.Generic;

namespace WagoLoader.Loader
{
    public class WagoSystem
    {
        public List<WagoProduct> Products { get; set; }
        public string Timezone { get; set; }
        public bool SetDateTime { get; set; }
    }
}