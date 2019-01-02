using System;
using System.Collections.Generic;
using System.IO;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace WagoLoader.Loader
{
    public class RemoteShell
    {
        private readonly string _controllerAddress;

        public RemoteShell(string controllerAddress)
        {
            _controllerAddress = controllerAddress;
        }

        public bool CheckRootLogin(string password)
        {
            try
            {
                using (var client = new SshClient(_controllerAddress, "root", password))
                {
                    client.Connect();

                    return client.IsConnected;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetRootPassword(List<string> passwords)
        {
            var shell = new RemoteShell(_controllerAddress);

            foreach (var password in passwords)
            {
                if (shell.CheckRootLogin(password))
                {
                    return password;
                }
            }

            Console.Write("Enter root password of controller: ");
            var pwd = Console.ReadLine();
            return shell.CheckRootLogin(pwd) ? pwd : null;
        }

        public string ExecCommand(string user, string password, string command)
        {
            try
            {
                using (var client = new SshClient(_controllerAddress, user, password))
                {
                    client.Connect();
                    if (!client.IsConnected)
                        return null;

                    var cmd = client.CreateCommand(command);
                    var response = cmd.Execute();
                    return response;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public bool ChangePassword(string rootPassword, string user, string newPassword)
        {
            var command = $"echo -e \"{newPassword}\n{newPassword}\" | passwd {user}";
            var result = ExecCommand("root", rootPassword, command);

            return result.Contains("changed");
        }

    }
}
