// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.AppManagement;
using Pixeval.Util.IO;

namespace Pixeval.Pages.Misc;

public record Supporter(string Nickname, string Name, Uri ProfilePicture, Uri ProfileUri)
{
    public static string BasePath { get; } = AppKnownFolders.Cache.CombinePath("GitHubSupporters");

    public string AtName => "@" + Name;

    public BitmapImage ProfileImage => new BitmapImage(new Uri(Path.Combine(BasePath, Name + ".png")));

    // ReSharper disable StringLiteralTypo
    private static readonly IEnumerable<(string Nickname, string Name)> _Supporters =
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
            ("irony", "kokoro-aya"),
            ("Betta_Fish", "zxbmmmmmmmmm"),
            ("Dylech30th", "dylech30th")
        }.OrderBy(_ => Random.Shared.Next());
    // ReSharper restore StringLiteralTypo

    public static List<Supporter>? Supporters { get; private set; }

    public static async IAsyncEnumerable<Supporter> GetSupportersAsync()
    {
        _ = Directory.CreateDirectory(BasePath);

        if (Supporters is null)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36 Edg/121.0.0.0");

            var path = Path.Combine(BasePath, "github-supporters.json");
            if (File.Exists(path))
            {
                await using var fileStream = IoHelper.OpenAsyncRead(path);

                if (await JsonSerializer.DeserializeAsync(fileStream, typeof(Supporter[]), SupporterSerializeContext.Default) is Supporter[] supporters
                    && supporters.Length == _Supporters.Count())
                {
                    Supporters = [.. supporters.OrderBy(_ => Random.Shared.Next())];
                    foreach (var supporter in Supporters)
                    {
                        await LoadAvatarAsync(supporter, BasePath, httpClient);
                        yield return supporter;
                    }
                    yield break;
                }
            }

            Supporters = [];
            foreach (var (nickname, name) in _Supporters)
                if (await httpClient.GetFromJsonAsync("https://api.github.com/users/" + name, typeof(GitHubUser),
                        GitHubUserSerializeContext.Default) is GitHubUser user)
                {
                    var supporter = new Supporter(nickname, name, user.AvatarUrl, user.HtmlUrl);
                    Supporters.Add(supporter);
                    await LoadAvatarAsync(supporter, BasePath, httpClient);
                    yield return supporter;
                }
            await using var fs = IoHelper.OpenAsyncWrite(path);
            await JsonSerializer.SerializeAsync(fs, Supporters, typeof(List<Supporter>), SupporterSerializeContext.Default);
        }
        else
            foreach (var supporter in Supporters)
                yield return supporter;
    }

    private static async ValueTask LoadAvatarAsync(Supporter supporter, string basePath, HttpClient client)
    {
        var path = Path.Combine(basePath, supporter.Name + ".png");
        if (File.Exists(path))
            return;
        var file = IoHelper.OpenAsyncWrite(path);
        if (await client.DownloadStreamAsync(file, supporter.ProfilePicture) is not null)
        {
            await file.DisposeAsync();
            File.Delete(path);
        }
        else
            await file.DisposeAsync();
    }
}

[JsonSerializable(typeof(GitHubUser))]
public partial class GitHubUserSerializeContext : JsonSerializerContext;

[JsonSerializable(typeof(Supporter[]))]
[JsonSerializable(typeof(List<Supporter>))]
public partial class SupporterSerializeContext : JsonSerializerContext;

public class GitHubUser
{
    [JsonPropertyName("avatar_url")]
    public required Uri AvatarUrl { get; init; }

    [JsonPropertyName("html_url")]
    public required Uri HtmlUrl { get; init; }
}
