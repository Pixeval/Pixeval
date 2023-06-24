#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/BookmarksPage.xaml.cs
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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Messages;
using Pixeval.Misc;
using Pixeval.Options;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Util.UI;

namespace Pixeval.Pages.Capability;

public sealed partial class BookmarksPage : ISortedIllustrationContainerPageHelper
{
    public BookmarksPage()
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

    public override void OnPageActivated(NavigationEventArgs navigationEventArgs)
    {
        PrivacyPolicyComboBox.SelectedItem = PrivacyPolicyComboBoxPublicItem;
        SortOptionComboBox.SelectedItem = MakoHelper.GetAppSettingDefaultSortOptionWrapper();
        WeakReferenceMessenger.Default.TryRegister<BookmarksPage, MainPageFrameNavigatingEvent>(this, static (recipient, _) => recipient.IllustrationContainer.ViewModel.DataProvider.FetchEngine?.Cancel());
    }

    private void BookmarksPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (MainWindow.GetNavigationModeAndReset() is not NavigationMode.Back)
        {
            ChangeSource();
        }
    }

    private void PrivacyPolicyComboBox_OnSelectionChangedWhenLoaded(object sender, SelectionChangedEventArgs e)
    {
        ChangeSource();
    }

    private void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ((ISortedIllustrationContainerPageHelper) this).OnSortOptionChanged();
    }

    private void ChangeSource()
    {
        _ = IllustrationContainer.ViewModel.ResetEngineAndFillAsync(App.AppViewModel.MakoClient.Bookmarks(App.AppViewModel.PixivUid!, PrivacyPolicyComboBox.GetComboBoxSelectedItemTag(PrivacyPolicy.Public), App.AppViewModel.AppSetting.TargetFilter));
    }

    private void SortOptionComboBoxContainer_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (App.AppViewModel.AppSetting.IllustrationViewOption is IllustrationViewOption.RiverFlow)
        {
            ToolTipService.SetToolTip(SortOptionComboBoxContainer, new ToolTip { Content = MiscResources.SortIsNotAllowedWithJustifiedLayout });
        }
    }
}
