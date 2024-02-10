#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/RankingsPage.xaml.cs
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

using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Misc;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.Generic;

namespace Pixeval.Pages.Capability;

public sealed partial class RankingsPage : ISortedIllustrationContainerPageHelper
{
    public RankingsPage()
    {
        InitializeComponent();
    }

    public DateTime MaxDate => DateTime.Now.AddDays(-2);

    public IllustrationContainer ViewModelProvider => IllustrationContainer;

    public SortOptionComboBox SortOptionProvider => SortOptionComboBox;

    public override void OnPageActivated(NavigationEventArgs navigationEventArgs)
    {
        SortOptionComboBox.SelectedItem = MakoHelper.GetAppSettingDefaultSortOptionWrapper();
        RankOptionComboBox.SelectedItem = LocalizedBoxHelper.Of<RankOption, RankOptionWrapper>(RankOption.Day);
        RankDateTimeCalendarDatePicker.Date = DateTime.Now.AddDays(-2);
        ChangeSource();
    }

    private void RankOptionComboBox_OnSelectionChangedWhenPrepared(object sender, SelectionChangedEventArgs e)
    {
        ChangeSource();
    }

    private void RankDateTimeCalendarDatePicker_OnDateChangedWhenLoaded(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
    {
        ChangeSource();
    }

    private void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ((ISortedIllustrationContainerPageHelper)this).OnSortOptionChanged();
    }

    private void ChangeSource()
    {
        var rankOption = (RankOptionComboBox.SelectedItem as RankOptionWrapper)?.Value ?? RankOption.Day;
        var dateTime = RankDateTimeCalendarDatePicker.Date?.DateTime ?? DateTime.Now.AddDays(-2);
        IllustrationContainer.ViewModel.ResetEngine(App.AppViewModel.MakoClient.Ranking(rankOption, dateTime));
    }
}
