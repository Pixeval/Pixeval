// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Model;
using Pixeval.Util.IO;
using Pixeval.Util.IO.Caching;

namespace Pixeval.Pages;

public partial class WorkInfoPageViewModel<T>(T entry) : ObservableObject where T : class, IWorkEntry
{
    public T Entry { get; } = entry;

    public UserInfo Illustrator { get; } = entry.User;

    public string? IllustrationDimensionText => Entry is Illustration illustration ? $"{illustration.Width} x {illustration.Height}" : null;

    [ObservableProperty]
    public partial ImageSource? AvatarSource { get; set; }

    public async Task LoadAvatarAsync()
    {
        if (Illustrator is { ProfileImageUrls.Medium: { } profileImage })
            AvatarSource = await App.AppViewModel.AppServiceProvider.GetRequiredService<MemoryCache>()
                .GetSourceFromMemoryCacheAsync(profileImage);
    }
}
