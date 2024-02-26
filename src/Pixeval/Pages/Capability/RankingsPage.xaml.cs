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
using Pixeval.Util;
using WinUI3Utilities;

namespace Pixeval.Pages.Capability;

public sealed partial class RankingsPage : IScrollViewProvider
{
    public RankingsPage()
    {
        InitializeComponent();
        RankOptionComboBox.ItemsSource = LocalizedResourceAttributeHelper.GetLocalizedResourceContents<RankOption>();
        RankOptionComboBox.SelectedItem = new StringRepresentableItem(RankOption.Day, null);
        RankDateTimeCalendarDatePicker.Date = MaxDate;
    }

    public DateTime MaxDate => DateTime.Now.AddDays(-2);

    public override void OnPageActivated(NavigationEventArgs navigationEventArgs)
    {
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

    private void ChangeSource()
    {
        var rankOption = RankOptionComboBox.SelectedItem.To<StringRepresentableItem>().Item.To<RankOption>();
        var dateTime = RankDateTimeCalendarDatePicker.Date!.Value.DateTime;
        IllustrationContainer.ViewModel.ResetEngine(App.AppViewModel.MakoClient.Ranking(rankOption, dateTime, App.AppViewModel.AppSettings.TargetFilter));
    }

    public ScrollView ScrollView => IllustrationContainer.ScrollView;
}
