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
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Database.Managers;
using Pixeval.Misc;

namespace Pixeval.Pages.Misc;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BrowsingHistoryPage : IScrollViewProvider
{
    public BrowsingHistoryPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs navigationEventArgs)
    {
        FetchEngine();
    }

    private void FetchEngine()
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = scope.ServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();
        IllustrationContainer.ViewModel.ResetEngine(
            App.AppViewModel.MakoClient.Computed(
                manager.Enumerate().Reverse().ToAsyncEnumerable()
                    .SelectAwait(async t => await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(t.Id))));
    }

    public ScrollView ScrollView => IllustrationContainer.ScrollView;
}
