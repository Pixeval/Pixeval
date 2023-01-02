#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/RecommendationPage.xaml.cs
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
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Util.UI;

namespace Pixeval.Pages.Capability;

public sealed partial class RecommendationPage : ISortedIllustrationContainerPageHelper
{
    public RecommendationPage()
    {
        InitializeComponent();
    }

    public IllustrationContainer ViewModelProvider => IllustrationContainer;

    public SortOptionComboBox SortOptionProvider => SortOptionComboBox;

    public override void OnPageDeactivated(NavigatingCancelEventArgs navigatingCancelEventArgs)
    {
        ModeSelectionComboBox.SelectionChangedWhenLoaded -= ModeSelectionComboBox_OnSelectionChangedWhenLoaded;
        SortOptionComboBox.SelectionChangedWhenLoaded -= SortOptionComboBox_OnSelectionChangedWhenLoaded;
        IllustrationContainer.ViewModel.Dispose();
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    public override void OnPageActivated(NavigationEventArgs navigationEventArgs)
    {
        ModeSelectionComboBox.SelectedItem = ModeSelectionComboBoxIllustComboBoxItem;
        SortOptionComboBox.SelectedItem = MakoHelper.GetAppSettingDefaultSortOptionWrapper();
        WeakReferenceMessenger.Default.Register<RecommendationPage, MainPageFrameNavigatingEvent>(this, (recipient, _) => recipient.IllustrationContainer.ViewModel.DataProvider.FetchEngine?.Cancel());
    }

    private void RecommendationsPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (MainWindow.GetNavigationModeAndReset() is not NavigationMode.Back)
        {
            ChangeSource();
        }
    }

    private void ModeSelectionComboBox_OnSelectionChangedWhenLoaded(object? sender, SelectionChangedEventArgs e)
    {
        ChangeSource();
    }

    private void SortOptionComboBox_OnSelectionChangedWhenLoaded(object sender, SelectionChangedEventArgs e)
    {
        // FUCK C#, the default implementations are not inherited. We have to use this stupid cast here.
        // even a donkey knows "this" is an "ISortedIllustrationContainerPageHelper"
        ((ISortedIllustrationContainerPageHelper)this).OnSortOptionChanged();
    }

    private void ChangeSource()
    {
        _ = IllustrationContainer.ViewModel.ResetEngineAndFillAsync(App.AppViewModel.MakoClient.Recommendations(ModeSelectionComboBox.GetComboBoxSelectedItemTag(RecommendationContentType.Illust)), App.AppViewModel.AppSetting.ItemsNumberLimitForDailyRecommendations);
    }
}