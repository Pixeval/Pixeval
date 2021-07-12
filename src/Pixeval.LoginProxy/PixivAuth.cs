using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Pixeval.LoginProxy
{
    public static class PixivAuth
    {
        public static string GetCodeVerify()
        {
            var bytes = new byte[32];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes); 
            return bytes.ToUrlSafeBase64String();
        }

        private static string ToUrlSafeBase64String(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd(new[] { '=' }).Replace("+", "-").Replace("/", "_");
        }

        public static string GenerateWebPageUrl(string codeVerify, bool signUp = false)
        {
            var codeChallenge = GetCodeChallenge(codeVerify);
            return signUp
                ? $"https://app-api.pixiv.net/web/v1/provisional-accounts/create?code_challenge={codeChallenge}&code_challenge_method=S256&client=pixiv-android"
                : $"https://app-api.pixiv.net/web/v1/login?code_challenge={codeChallenge}&code_challenge_method=S256&client=pixiv-android";
        }

        private static string GetCodeChallenge(string code)
        {
            return code.HashBytes<SHA256CryptoServiceProvider>(Encoding.ASCII).ToUrlSafeBase64String();
        }

        private static byte[] HashBytes<T>(this string str, Encoding? encoding = null) where T : HashAlgorithm, new()
        {
            using var crypt = new T();
            var hashBytes = crypt.ComputeHash((encoding ?? Encoding.UTF8).GetBytes(str));
            return hashBytes;
        }
    }
}