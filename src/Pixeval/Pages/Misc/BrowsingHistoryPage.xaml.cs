#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/BrowsingHistoryPage.xaml.cs
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

using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Database.Managers;
using Pixeval.Utilities;

namespace Pixeval.Pages.Misc;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BrowsingHistoryPage : IScrollViewHost
{
    public BrowsingHistoryPage() => InitializeComponent();

    private void BrowsingHistoryPage_OnLoaded(object sender, RoutedEventArgs e) => ChangeSource();

    private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => ChangeSource();

    private void ChangeSource()
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();
        var type = SimpleWorkTypeComboBox.GetSelectedItem<SimpleWorkType>();
        var source = manager.Reverse()
            .SelectNotNull(t => t.TryGetEntry(type))
            .ToAsyncEnumerable();

        WorkContainer.WorkView.ResetEngine(type switch
        {
            SimpleWorkType.IllustAndManga => App.AppViewModel.MakoClient.Computed(source.Cast<Illustration>()),
            _ => App.AppViewModel.MakoClient.Computed(source.Cast<Novel>()),
        });
    }

    public ScrollView ScrollView => WorkContainer.ScrollView;
}
