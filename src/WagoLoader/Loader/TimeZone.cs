using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WagoLoader.Loader
{
    public class Timezone
    {
        public static bool Validate(string timezone)
        {
            // ReSharper disable CommentTypo
            // See embedded resource "allzones"
            // or on device /usr/sharezoneinfo/allzones
            // ReSharper restore CommentTypo

            var assembly = typeof(Timezone).GetTypeInfo().Assembly;

            var name = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("allzones"));
            Debug.Assert(name != null);

            using (var resource = assembly.GetManifestResourceStream(name))
            {
                if (resource != null)
                {
                    using (var rdr = new StreamReader(resource))
                    {
                        while (true)
                        {
                            var zone = rdr.ReadLine()?.Split(' ');
                            if (zone == null) break;
                            if (zone[0] == timezone) return true;
                        }
                    }
                }
            }

            return false;
        }

        public static void SetTimezone(RemoteShell shell, string rootPwd, string timezone)
        {
            // with root access execute
            // /etc/config-tools/config_timezone timezone=<timezone-value>

            var cmd = $"/etc/config-tools/config_timezone timezone={timezone}";
            shell.ExecCommand("root", rootPwd, cmd);
        }

    }
}
