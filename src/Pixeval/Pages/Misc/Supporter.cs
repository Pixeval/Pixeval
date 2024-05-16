#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/Supporter.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Pixeval.Pages.Misc;

public record Supporter(string Nickname, string Name, ImageSource ProfilePicture, Uri ProfileUri)
{
    // ReSharper disable StringLiteralTypo
    private static readonly IEnumerable<(string Nickname, string Name)> _supporters =
        new (string Nickname, string Name)[]
        {
            ("Sep", "Guro2"), 
            ("无论时间", "wulunshijian"), 
            ("CN", "ControlNet"),
            ("CY", "Cyl18"),
            ("对味", "duiweiya"),
            ("LG", "LasmGratel"),
            ("鱼鱼", "sovetskyfish"),
            ("探姬", "PerolNotsfsssf"),
            ("Summpot", "Summpot"),
            ("扑克", "Poker-sang"),
            ("南门二", "Rigil-Kentaurus"),
            ("当妈", "TheRealKamisama"),
            ("茶栗", "cqjjjzr"),
            ("cnbluefire", "cnbluefire"),
            ("岛风", "frg2089"),
            ("Ёж, просто ёж", "bropines"),
            ("kokoro-aya", "irol")
        }.OrderBy(_ => Random.Shared.Next());
    // ReSharper restore StringLiteralTypo

    public static List<Supporter> Supporters { get; } = [];

    public static async IAsyncEnumerable<Supporter> GetSupportersAsync(HttpClient httpClient, bool dispose = true)
    {
        if (Supporters is [])
        {
            if (httpClient.DefaultRequestHeaders.UserAgent.Count is 0)
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36 Edg/121.0.0.0");
            foreach (var (nickname, name) in _supporters)
                if (await httpClient.GetFromJsonAsync<GitHubUser>("https://api.github.com/users/" + name) is { } user)
                {
                    var supporter = new Supporter(nickname, '@' + name, new BitmapImage(user.AvatarUrl), user.HtmlUrl);
                    Supporters.Add(supporter);
                    yield return supporter;
                }
        }
        else
        {
            foreach (var supporter in Supporters)
                yield return supporter;
        }
        if (dispose)
            httpClient.Dispose();
    }
}

file class GitHubUser
{
    [JsonPropertyName("avatar_url")]
    public required Uri AvatarUrl { get; init; }

    [JsonPropertyName("html_url")]
    public required Uri HtmlUrl { get; init; }
}
