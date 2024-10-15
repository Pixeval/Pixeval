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
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;
using WinRT;
using WinUI3Utilities;

namespace Pixeval.Pages.Capability;

public sealed partial class RankingsPage : IScrollViewHost
{
    private static readonly IReadOnlyList<StringRepresentableItem> _illustrationRankOption =  RankOptionExtension.GetItems();

    private static readonly IReadOnlyList<StringRepresentableItem> _novelRankOption = NovelRankOptionExtension.GetItems();

    public RankingsPage()
    {
        InitializeComponent();
        RankDateTimeCalendarDatePicker.Date = MaxDate;
    }

    public DateTime MaxDate => DateTime.Now.AddDays(-2);

    private void RankingsPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        ChangeEnumSource();
        ChangeSource();
    }

    private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ChangeEnumSource();
        ChangeSource();
    }

    private void ChangeEnumSource()
    {
        if (SimpleWorkTypeComboBox.GetSelectedItem<SimpleWorkType>() is SimpleWorkType.IllustAndManga)
        {
            RankOptionComboBox.ItemsSource = _illustrationRankOption;
            RankOptionComboBox.SelectedItem = _illustrationRankOption.First(r => Equals(r.Item, App.AppViewModel.AppSettings.IllustrationRankOption));
        }
        else
        {
            RankOptionComboBox.ItemsSource = _novelRankOption;
            RankOptionComboBox.SelectedItem = _novelRankOption.First(r => Equals(r.Item, App.AppViewModel.AppSettings.NovelRankOption));
        }
    }

    private void OnSelectionChanged(object sender, IWinRTObject e) => ChangeSource();

    private void ChangeSource()
    {
        var rankOption = RankOptionComboBox.SelectedItem.To<StringRepresentableItem>().Item.To<RankOption>();
        var dateTime = RankDateTimeCalendarDatePicker.Date!.Value.DateTime;
        WorkContainer.WorkView.ResetEngine(SimpleWorkTypeComboBox.GetSelectedItem<SimpleWorkType>() is SimpleWorkType.IllustAndManga
            ? App.AppViewModel.MakoClient.IllustrationRanking(rankOption, dateTime, App.AppViewModel.AppSettings.TargetFilter)
            : App.AppViewModel.MakoClient.NovelRanking(rankOption, dateTime, App.AppViewModel.AppSettings.TargetFilter));
    }

    public ScrollView ScrollView => WorkContainer.ScrollView;
}
