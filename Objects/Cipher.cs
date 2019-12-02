using System;
using System.Security.Cryptography;
using System.Text;

namespace Pzxlane.Objects
{
    public class Cipher
    {
        public static string Md5Hex(string input)
        {
            using var hasher = new MD5CryptoServiceProvider();
            var hash = hasher.ComputeHash(input.GetBytes());

            var stringBuilder = new StringBuilder();
            foreach (var b in hash)
            {
                stringBuilder.Append(b.ToString("x2"));
            }

            return stringBuilder.ToString();
        }
    }
}