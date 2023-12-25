#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustratorMangaPage.xaml.cs
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
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Model;
using Pixeval.Messages;
using Pixeval.Misc;
using Pixeval.Util;
using Pixeval.Utilities;

namespace Pixeval.Controls.IllustratorContentViewer;

public sealed partial class IllustratorMangaPage : ISortedIllustrationContainerPageHelper, IIllustratorContentViewerCommandBarHostSubPage
{
    public IllustratorMangaPage()
    {
        InitializeComponent();
    }

    public void Dispose()
    {
        IllustrationContainer.ViewModel.Dispose();
    }

    public void PerformSearch(string keyword)
    {
        if (IllustrationContainer.ShowCommandBar)
        {
            return;
        }

        IllustrationContainer.ViewModel.DataProvider.View.Filter = keyword.IsNullOrBlank()
            ? null
            : o => o.Id.ToString().Contains(keyword)
                   || (o.Illustrate.Tags ??
                       Enumerable.Empty<Tag>()).Any(x =>
                       x.Name.Contains(keyword) ||
                       (x.TranslatedName?.Contains(keyword) ?? false))
                   || (o.Illustrate.Title?.Contains(keyword) ??
                       false);
    }

    public void ChangeCommandBarVisibility(bool isVisible)
    {
        IllustrationContainer.ShowCommandBar = isVisible;
    }

    public IllustrationContainer ViewModelProvider => IllustrationContainer;

    public SortOptionComboBox SortOptionProvider => SortOptionComboBox;

    public override void OnPageActivated(NavigationEventArgs e)
    {
        if (ActivationCount > 1)
            return;

        _ = WeakReferenceMessenger.Default.TryRegister<IllustratorMangaPage, MainPageFrameNavigatingEvent>(this, static (recipient, _) => recipient.IllustrationContainer.ViewModel.DataProvider.FetchEngine?.Cancel());
        if (e.Parameter is string id)
        {
            IllustrationContainer.ViewModel.ResetEngine(App.AppViewModel.MakoClient.MangaPosts(id, App.AppViewModel.AppSetting.TargetFilter));
        }

        if (!App.AppViewModel.AppSetting.ShowExternalCommandBarInIllustratorContentViewer)
        {
            ChangeCommandBarVisibility(false);
        }
    }

    public override void OnPageDeactivated(NavigatingCancelEventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    private void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ((ISortedIllustrationContainerPageHelper)this).OnSortOptionChanged();
    }
}
