#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/RecommendationPage.xaml.cs
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
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Misc;
using Pixeval.Util;
using Pixeval.Util.UI;

namespace Pixeval.Pages.Capability;

public sealed partial class RecommendationPage : ISortedIllustrationContainerPageHelper
{
    public RecommendationPage() => InitializeComponent();

    public IllustrationContainer ViewModelProvider => IllustrationContainer;

    public SortOptionComboBox SortOptionProvider => SortOptionComboBox;

    public override void OnPageDeactivated(NavigatingCancelEventArgs e)
    {
        ModeSelectionComboBox.SelectionChangedWhenLoaded -= ModeSelectionComboBox_OnSelectionChangedWhenLoaded;
        SortOptionComboBox.SelectionChangedWhenLoaded -= SortOptionComboBox_OnSelectionChanged;
    }

    public override void OnPageActivated(NavigationEventArgs e)
    {
        ModeSelectionComboBox.SelectedItem = ModeSelectionComboBoxIllustComboBoxItem;
        SortOptionComboBox.SelectedItem = MakoHelper.GetAppSettingDefaultSortOptionWrapper();
        ChangeSource();
    }

    private void ModeSelectionComboBox_OnSelectionChangedWhenLoaded(object? sender, SelectionChangedEventArgs e)
    {
        ChangeSource();
    }

    private void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // FUCK C#, the default implementations are not inherited. We have to use this stupid cast here.
        // even a donkey knows "this" is an "ISortedIllustrationContainerPageHelper"
        ((ISortedIllustrationContainerPageHelper)this).OnSortOptionChanged();
    }

    private void ChangeSource()
    {
        IllustrationContainer.ViewModel.ResetEngine(App.AppViewModel.MakoClient.Recommendations(ModeSelectionComboBox.GetComboBoxSelectedItemTag(RecommendationContentType.Illust), App.AppViewModel.AppSettings.TargetFilter), App.AppViewModel.AppSettings.ItemsNumberLimitForDailyRecommendations);
    }
}
