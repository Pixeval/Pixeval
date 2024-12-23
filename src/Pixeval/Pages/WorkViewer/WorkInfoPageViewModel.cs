#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/WorkInfoPageViewModel.cs
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
