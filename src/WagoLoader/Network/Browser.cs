using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace WagoLoader.Network
{
    public class Browser
    {
        public static List<IPAddress> FindIpV4Devices()
        {
            var ipV4Addresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                .ToList();

            var addresses = new List<IPAddress>();
            foreach (var ipAddress in ipV4Addresses)
            {
                var bytes = ipAddress.GetAddressBytes();
                for (var sub = 1; sub < 255; sub++)
                {
                    addresses.Add(IPAddress.Parse($"{bytes[0]}.{bytes[1]}.{bytes[2]}.{sub}"));
                }
            }

            var tasks = addresses.Select(ip => new Ping().SendPingAsync(ip, 500));
            var results = Task.WhenAll(tasks).Result;

            addresses = results
                .Where(res => res.Status == IPStatus.Success)
                .Select(res => res.Address)
                .ToList();

            return addresses;
        }
    }
}
