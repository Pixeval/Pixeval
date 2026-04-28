// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Security.Cryptography;
using System.Text;

namespace Pixeval.Pages.Login;

public static class PixivAuth
{
    public static string GetCodeVerify()
    {
        return RandomNumberGenerator.GetBytes(32).ToUrlSafeBase64String();
    }

    private static string ToUrlSafeBase64String(this byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace("+", "-").Replace("/", "_");
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
        return SHA256.HashData(Encoding.ASCII.GetBytes(code)).ToUrlSafeBase64String();
    }
}
