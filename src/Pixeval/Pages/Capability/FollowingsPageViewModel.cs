#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/FollowingsPageViewModel.cs
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

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pixeval.Controls.IllustratorView;
using Pixeval.CoreApi.Global.Enum;

namespace Pixeval.Pages.Capability;

public partial class FollowingsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<IllustratorViewModel> _illustrators;

    public FollowingsPageViewModel()
    {
        _illustrators = new ObservableCollection<IllustratorViewModel>();
    }

    public async Task LoadFollowings()
    {
        var fetchEngine = App.AppViewModel.MakoClient.Following(App.AppViewModel.PixivUid!, PrivacyPolicy.Public);
        await foreach (var user in fetchEngine)
        {
            var model = new IllustratorViewModel(user.UserInfo!);
            _illustrators.Add(model);
            _ = model.LoadAvatarSource();
        }
    }
}