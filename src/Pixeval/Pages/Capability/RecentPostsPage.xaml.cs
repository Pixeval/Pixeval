﻿#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/RecentPostsPage.xaml.cs
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

using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Messages;
using Pixeval.Misc;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Util.UI;

namespace Pixeval.Pages.Capability;

public sealed partial class RecentPostsPage : ISortedIllustrationContainerPageHelper
{
    public RecentPostsPage()
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
        WeakReferenceMessenger.Default.Register<RecentPostsPage, MainPageFrameNavigatingEvent>(this, (recipient, _) => recipient.IllustrationContainer.ViewModel.FetchEngine?.Cancel());
    }

    private void RecentPostsPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (App.AppViewModel.Window.GetNavigationModeAndReset() is not NavigationMode.Back)
        {
            ChangeSource();
        }
    }

    private void PrivacyPolicyComboBox_OnSelectionChangedWhenLoaded(object sender, SelectionChangedEventArgs e)
    {
        ChangeSource();
    }

    private void ChangeSource()
    {
        _ = IllustrationContainer.ViewModel.VisualizationController.ResetAndFillAsync(App.AppViewModel.MakoClient.RecentPosts(PrivacyPolicyComboBox.GetComboBoxSelectedItemTag(PrivacyPolicy.Public)));
    }

    private void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ((ISortedIllustrationContainerPageHelper) this).OnSortOptionChanged();
    }
}