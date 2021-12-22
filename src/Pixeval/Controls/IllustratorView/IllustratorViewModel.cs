#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IllustratorViewModel.cs
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
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Pixeval.AppManagement;
using Pixeval.CoreApi.Model;
using Pixeval.Util.IO;
using Pixeval.Utilities;

namespace Pixeval.Controls.IllustratorView;

public partial class IllustratorViewModel : ObservableObject
{
    [ObservableProperty]
    private ImageSource? _avatarSource;

    public IllustratorViewModel(UserInfo info)
    {
        Name = info.Name!;
        AvatarUrl = info.ProfileImageUrls?.Medium!;
        Id = info.Id;
        Account = info.Account;
        Comment = info.Comment;
    }

    public string Name { get; set; }

    public string AvatarUrl { get; set; }

    public long Id { get; set; }

    public string? Account { get; set; }

    public string? Comment { get; set; }

    public async Task LoadAvatarSource()
    {
        if (AvatarSource != null)
        {
            return;
        }

        AvatarSource = (await App.AppViewModel.MakoClient.DownloadBitmapImageResultAsync(AvatarUrl).GetOrElseAsync(await AppContext.GetPixivNoProfileImageAsync()))!;
    }
}