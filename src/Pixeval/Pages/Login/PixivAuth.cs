// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.Util;
using System.Threading.Tasks;
using Pixeval.CoreApi;
using Pixeval.Util.IO;

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

    public static async Task<TokenResponse> AuthCodeToTokenResponseAsync(string code, string verifier)
    {
        // HttpClient is designed to be used through whole application lifetime, create and
        // dispose it in a function is a commonly misused anti-pattern, but this function
        // is intended to be called only once (at the start time) during the entire application's
        // lifetime, so the overhead is acceptable

        var httpClient = App.AppViewModel.AppSettings.EnableDomainFronting
            ? new HttpClient(new DelegatedHttpMessageHandler(MakoHttpOptions.CreateHttpMessageInvoker()))
            : new();
        httpClient.DefaultRequestHeaders.UserAgent.Add(new("PixivAndroidApp", "5.0.64"));
        httpClient.DefaultRequestHeaders.UserAgent.Add(new("(Android 6.0)"));
        var scheme = App.AppViewModel.AppSettings.EnableDomainFronting ? "http" : "https";

        using var result = await httpClient.PostFormAsync(scheme + "://oauth.secure.pixiv.net/auth/token",
            ("code", code),
            ("code_verifier", verifier),
            ("client_id", "MOBrBDS8blbauoSck0ZfDbtuzpyT"),
            ("client_secret", "lsACyCD94FhDUtGTXi3QzcFE2uU1hqtDaKeqrdwj"),
            ("grant_type", "authorization_code"),
            ("include_policy", "true"),
            ("redirect_uri", "https://app-api.pixiv.net/web/v1/users/auth/pixiv/callback"));
        // using会有resharper警告，所以这里用Dispose
        httpClient.Dispose();
        _ = result.EnsureSuccessStatusCode();
        var str = await result.Content.ReadAsStringAsync();
        var tokenResponse = (TokenResponse) JsonSerializer.Deserialize(str, typeof(TokenResponse), AppJsonSerializerContext.Default)!;
        App.AppViewModel.LoginContext.RefreshToken = tokenResponse.RefreshToken;
        App.AppViewModel.LoginContext.IsPremium = tokenResponse.Response?.User.IsPremium ?? false;
        return tokenResponse;
    }
}
