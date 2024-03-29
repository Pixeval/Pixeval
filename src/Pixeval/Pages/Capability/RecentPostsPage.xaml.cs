#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/RecentPostsPage.xaml.cs
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
using Pixeval.Misc;
using Pixeval.Options;

namespace Pixeval.Pages.Capability;

public sealed partial class RecentPostsPage : IScrollViewProvider
{
    public RecentPostsPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e)
    {
        ChangeSource();
    }

    private void ComboBox_OnSelectionChangedWhenLoaded(object sender, SelectionChangedEventArgs e)
    {
        ChangeSource();
    }

    private void ChangeSource()
    {
        WorkContainer.WorkView.ResetEngine(SimpleWorkTypeComboBox.SelectedItem is SimpleWorkType.IllustAndManga
            ? App.AppViewModel.MakoClient.RecentIllustrationPosts(PrivacyPolicyComboBox.SelectedItem)
            : App.AppViewModel.MakoClient.RecentNovelPosts(PrivacyPolicyComboBox.SelectedItem));
    }

    public ScrollView ScrollView => WorkContainer.ScrollView;
}
