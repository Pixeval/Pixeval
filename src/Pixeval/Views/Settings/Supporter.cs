// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.AppManagement;
using Pixeval.Utilities;
using Pixeval.Utilities.IO;
using Pixeval.ViewModels;

namespace Pixeval.Views.Settings;

public partial class Supporter : ViewModelBase
{
    public Supporter() // JsonSerializer use
    {
    }

    public Supporter(string nickname, string name)
    {
        Nickname = nickname;
        Name = name;
    }

    static Supporter()
    {
        Random.Shared.Shuffle(Supporters);
    }

    public static string BasePath { get; } = Path.Combine(AppInfo.CacheFolder, "GitHubSupporters");

    public string AtName => "@" + Name;

    public Bitmap? ProfileImage
    {
        get
        {
            var path = Path.Combine(BasePath, Name + ".png");
            if (!File.Exists(path))
                return null;

            try
            {
                return new Bitmap(path);
            }
            catch
            {
                _ = FileHelper.TryDeleteFile(path);
                return null;
            }
        }
    }

    public string Nickname { get; init; } = null!;

    public string Name { get; init; } = null!;

    public string? LocalProfilePicture
    {
        get
        {
            var path = Path.Combine(BasePath, Name + ".png");
            return File.Exists(path) ? path : null;
        }
    }

    [ObservableProperty]
    public partial Uri? ProfilePicture { get; set; }

    [ObservableProperty]
    public partial Uri? ProfileUri { get; set; }

    // ReSharper disable StringLiteralTypo
    public static Supporter[] Supporters { get; } =
    [
        new("探姬", "PerolNotsfsssf"),
        new("Summpot", "Summpot"),
        new("扑克", "Poker-sang"),
        new("Shirasagi", "Shirasagi0012"),
        new("cnbluefire", "cnbluefire"),
        new("岛风", "frg2089"),
        new("Ёж, просто ёж", "bropines"),
        new("irony", "kokoro-aya"),
        new("Betta_Fish", "zxbmmmmmmmmm"),
        new("Dylech30th", "dylech30th")
    ];
    // ReSharper restore StringLiteralTypo

    public static async Task GetSupportersAsync()
    {
        try
        {

            _ = Directory.CreateDirectory(BasePath);

            var httpClient = App.AppViewModel.GetRequiredGitHubHttpClient();

            var path = Path.Combine(BasePath, "github-supporters.json");
            var exists = File.Exists(path);
            if (exists)
            {
                await using var fileStream = File.OpenAsyncRead(path);

                if (await JsonSerializer.DeserializeAsync(fileStream, GitHubUserSerializeContext.Default.SupporterArray) is { } supporters
                    && supporters.Length == Supporters.Length)
                {
                    foreach (var supporter in Supporters)
                        await LoadAvatarAsync(supporter, BasePath, httpClient);

                    return;
                }
            }

            foreach (var supporter in Supporters)
                if (await httpClient.GetFromJsonAsync("https://api.github.com/users/" + supporter.Name, GitHubUserSerializeContext.Default.GitHubUser) is { } user)
                {
                    supporter.ProfilePicture = user.AvatarUrl;
                    supporter.ProfileUri = user.HtmlUrl;
                    await LoadAvatarAsync(supporter, BasePath, httpClient);
                }

            if (!exists)
            {
                await using var fs = File.CreateAsyncWrite(path);
                await JsonSerializer.SerializeAsync(fs, Supporters, GitHubUserSerializeContext.Default.SupporterArray);
            }
        }
        catch
        {
            // ignored
        }

        return;

        static async ValueTask LoadAvatarAsync(Supporter supporter, string basePath, HttpClient client)
        {
            var path = Path.Combine(basePath, supporter.Name + ".png");
            if (File.Exists(path) || supporter.ProfilePicture is null)
                return;
            var file = File.CreateAsyncWrite(path);
            if (await client.DownloadStreamAsync(file, supporter.ProfilePicture) is not null)
            {
                await file.DisposeAsync();
                File.Delete(path);
            }
            else
            {
                await file.DisposeAsync();
                supporter.OnPropertyChanged(nameof(LocalProfilePicture));
            }
        }
    }
}

[JsonSerializable(typeof(GitHubUser))]
[JsonSerializable(typeof(Supporter[]))]
public partial class GitHubUserSerializeContext : JsonSerializerContext;

public class GitHubUser
{
    [JsonPropertyName("avatar_url")]
    public required Uri AvatarUrl { get; init; }

    [JsonPropertyName("html_url")]
    public required Uri HtmlUrl { get; init; }
}
