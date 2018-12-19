using System;
using System.Collections.Generic;
using Renci.SshNet;

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

    }
}
