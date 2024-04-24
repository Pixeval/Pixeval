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
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Database.Managers;
using Pixeval.Misc;

namespace Pixeval.Pages.Misc;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BrowsingHistoryPage : IScrollViewProvider
{
    public BrowsingHistoryPage() => InitializeComponent();

    private void BrowsingHistoryPage_OnLoaded(object sender, RoutedEventArgs e) => ChangeSource();

    private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => ChangeSource();

    private void ChangeSource()
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = scope.ServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();
        var source = manager.Enumerate().Where(t => t.Type == SimpleWorkTypeComboBox.GetSelectedItem<SimpleWorkType>())
            .Reverse()
            .ToAsyncEnumerable();
        // 由于 ResetEngine 需要根据泛型参数判断类型，所以不能将元素转为 IWorkEntry，而是要保持原始类型
        WorkContainer.WorkView.ResetEngine(
            SimpleWorkTypeComboBox.GetSelectedItem<SimpleWorkType>() switch
            {
                SimpleWorkType.IllustAndManga => App.AppViewModel.MakoClient.Computed(source.SelectAwait(async t =>
                    await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(t.Id))),
                _ => App.AppViewModel.MakoClient.Computed(source.SelectAwait(async t =>
                    await App.AppViewModel.MakoClient.GetNovelFromIdAsync(t.Id)))
            });

    }

    public ScrollView ScrollView => WorkContainer.ScrollView;
}
