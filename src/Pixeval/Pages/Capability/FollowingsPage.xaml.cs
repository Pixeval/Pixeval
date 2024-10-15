#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/FollowingsPage.xaml.cs
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

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;

namespace Pixeval.Pages.Capability;

public sealed partial class FollowingsPage : IScrollViewHost
{
    public FollowingsPage() => InitializeComponent();

    private long _uid = -1;

    private bool IsMe => _uid == App.AppViewModel.PixivUid;

    public override void OnPageActivated(NavigationEventArgs e)
    {
        if (e.Parameter is not long uid)
            uid = App.AppViewModel.PixivUid;
        _uid = uid;
        ChangeSource();
    }

    private void PrivacyPolicyComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ChangeSource();
    }

    private void ChangeSource()
    {
        IllustratorView.ViewModel.ResetEngine(App.AppViewModel.MakoClient.Following(_uid, PrivacyPolicyComboBox.GetSelectedItem<PrivacyPolicy>()));
    }

    public ScrollView ScrollView => IllustratorView.ScrollView;
}
