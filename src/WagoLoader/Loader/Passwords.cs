using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WagoLoader.Loader
{
    public class Passwords
    {
        private readonly string _fileName;

        public Passwords(string fileName)
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

        private const string B64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                                   "abcdefghijklmnopqrstuvwxyz" +
                                   "0123456789+/";
        private string EncodeB64(byte[] data)
        {
            var encoded = "";
            for (var ix = 0; ix < data.Length; ix += 3)
            {
                var b0 = (long)data[ix];
                var b1 = (ix + 1) < data.Length ? (long)data[ix+1] : 0;
                var b2 = (ix + 2) < data.Length ? (long)data[ix+2] : 0;
                var x = (b0 << 16) + (b1 << 8) + b2;
                var y0 = (int)(x >> 18) & 0x3F;
                var y1 = (int)(x >> 12) & 0x3F;
                var y2 = (int)(x >> 6) & 0x3F;
                var y3 = (int)x & 0x3F;
                encoded += B64Alphabet[y0];
                encoded += B64Alphabet[y1];
                encoded += B64Alphabet[y2];
                encoded += B64Alphabet[y3];
            }

            return encoded;
        }

        private byte[] DecodeB64(string text)
        {
            var data = new byte[(text.Length / 4) * 3];
            var dx = 0;
            for (var ix = 0; ix < text.Length; ix += 4)
            {
                var y0 = B64Alphabet.IndexOf(text[ix]);
                var y1 = B64Alphabet.IndexOf(text[ix+1]);
                var y2 = B64Alphabet.IndexOf(text[ix+2]);
                var y3 = B64Alphabet.IndexOf(text[ix+3]);
                var x = (y0 << 18) + (y1 << 12) + (y2 << 6) + y3;
                var b0 = (byte)((x >> 16) & 0xFF);
                var b1 = (byte)((x >> 8) & 0xFF);
                var b2 = (byte)(x & 0xFF);
                data[dx++] = b0;
                data[dx++] = b1;
                data[dx++] = b2;
            }
            return data;
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
