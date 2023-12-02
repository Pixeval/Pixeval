#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/SearchResultsPage.xaml.cs
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

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Messages;
using Pixeval.Misc;
using Pixeval.Controls;
using Pixeval.Util;

namespace Pixeval.Pages.Capability;

public sealed partial class SearchResultsPage : ISortedIllustrationContainerPageHelper
{
    public SearchResultsPage()
    {
        InitializeComponent();
    }

    public IllustrationContainer ViewModelProvider => IllustrationContainer;

    public SortOptionComboBox SortOptionProvider => SortOptionComboBox;

    public override void OnPageDeactivated(NavigatingCancelEventArgs navigatingCancelEventArgs)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    public override void OnPageActivated(NavigationEventArgs navigationEventArgs)
    {
        SortOptionComboBox.SelectedItem = MakoHelper.GetAppSettingDefaultSortOptionWrapper();
        ChangeSource((IFetchEngine<Illustration>)navigationEventArgs.Parameter);
        _ = WeakReferenceMessenger.Default.TryRegister<SearchResultsPage, MainPageFrameNavigatingEvent>(this, static (recipient, _) => recipient.IllustrationContainer.ViewModel.DataProvider.FetchEngine?.Cancel());
    }

    private void ChangeSource(IFetchEngine<Illustration> engine)
    {
        IllustrationContainer.ViewModel.ResetEngine(engine);
    }

    private void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ((ISortedIllustrationContainerPageHelper)this).OnSortOptionChanged();
    }
}
