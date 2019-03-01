using System;

namespace WagoLoader.Loader
{
    public class Clock
    {
        public static string SetDateTime(RemoteShell shell, string rootPwd, DateTime dateTime)
        {
            // with root access execute
            // /etc/config-tools/config_clock < type=time-type-value time=time-value | date=date-value >

            var type = (dateTime.Kind == DateTimeKind.Utc) ? "utc" : "local";
            var date = dateTime.ToString("dd.MM.yyyy");
            var time = dateTime.ToString("hh:mm:ss");
            var cmd = $"/etc/config-tools/config_clock type={type} date={date} time={time}";
            return shell.ExecCommand("root", rootPwd, cmd);
        }

    }
}
