#region Copyright (c) Pixeval/Pixeval.LoginProxy

// GPL v3 License
// 
// Pixeval/Pixeval.LoginProxy
// Copyright (c) 2021 Pixeval.LoginProxy/PixivAuth.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Security.Cryptography;
using System.Text;

namespace Pixeval.LoginProxy;

public static class PixivAuth
{
    public static string GetCodeVerify()
    {
        return RandomNumberGenerator.GetBytes(32).ToUrlSafeBase64String();
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
        return SHA256.HashData(Encoding.ASCII.GetBytes(code)).ToUrlSafeBase64String();
    }
}