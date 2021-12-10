#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/BrowsingHistoryPage.xaml.cs
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Database.Managers;
using Pixeval.Messages;
using Pixeval.Misc;
using Pixeval.Pages.Capability;
using Pixeval.UserControls;
using Pixeval.Util;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pixeval.Pages.Misc;

/// <summary>
///     An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BrowsingHistoryPage : ISortedIllustrationContainerPageHelper
{
    public BrowsingHistoryPage()
    {
        InitializeComponent();
    }

    public IllustrationContainer ViewModelProvider => IllustrationContainer;

    public SortOptionComboBox SortOptionProvider => SortOptionComboBox;

    public override void OnPageDeactivated(NavigatingCancelEventArgs navigatingCancelEventArgs)
    {
        IllustrationContainer.ViewModel.Dispose();
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    public override async void OnPageActivated(NavigationEventArgs navigationEventArgs)
    {
        SortOptionComboBox.SelectedItem = MakoHelper.GetAppSettingDefaultSortOptionWrapper();
        await FetchAndFill();
        WeakReferenceMessenger.Default.Register<BrowsingHistoryPage, MainPageFrameNavigatingEvent>(this, (recipient, _) => recipient.IllustrationContainer.ViewModel.FetchEngine?.Cancel());
    }

    private async Task FetchAndFill()
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = await scope.ServiceProvider.GetRequiredService<Task<BrowseHistoryPersistentManager>>();
        foreach (var entry in await manager.SelectAsync(count: 100))
        {
            var illustration = await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(entry.Id!);
            IllustrationContainer!.ViewModel.AddIllustrationViewModel(new IllustrationViewModel(illustration));
        }
    }

    private void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ((ISortedIllustrationContainerPageHelper)this).OnSortOptionChanged();
    }
}