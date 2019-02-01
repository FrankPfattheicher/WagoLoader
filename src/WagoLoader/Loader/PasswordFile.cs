using System.Collections.Generic;
using System.IO;
using System.Text;
// ReSharper disable UnusedMember.Global

namespace WagoLoader.Loader
{
    public class PasswordFile
    {
        private readonly string _fileName;

        public PasswordFile(string fileName)
        {
            _fileName = fileName;
        }

        public List<string> GetUsers()
        {
            var users = new List<string>();
            var lines = File.ReadAllLines(_fileName);
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var line in lines)
            {
                var userPw = line.Split(':');
                if (userPw.Length != 2) continue;

                users.Add(userPw[0]);
            }
            return users;
        }

        public bool IsValid(string user, string password)
        {
            if (string.IsNullOrEmpty(password)) return false;

            var lines = File.ReadAllLines(_fileName);
            foreach (var line in lines)
            {
                var userPw = line.Split(':');
                if (userPw.Length != 2) continue;
                if (userPw[0] != user) continue;

                return Password.Verify(password, userPw[1]);
            }

            return false;
        }

        private List<string> LinesWithoutUser(string user)
        {
            var newLines = new List<string>();
            var lines = File.ReadAllLines(_fileName);
            foreach (var line in lines)
            {
                var userPw = line.Split(':');
                if (userPw.Length != 2) continue;
                if (userPw[0] != user)
                {
                    newLines.Add(line);
                }
            }
            return newLines;
        }

        public void RemoveUser(string user)
        {
            var newLines = LinesWithoutUser(user);
            File.WriteAllLines(_fileName, newLines);
        }

        public void SetPassword(string user, string password)
        {
            var newLines = LinesWithoutUser(user);

            var hash = Password.Create(password);
            var newLine = $"{user}:{hash}";
            newLines.Add(newLine);

            var text = string.Join("\n", newLines) + "\n";
            var utf8WithoutBom = new UTF8Encoding(false);
            using (var writer = new StreamWriter(_fileName, false, utf8WithoutBom))
            {
                writer.Write(text);
            }
        }

    }
}
