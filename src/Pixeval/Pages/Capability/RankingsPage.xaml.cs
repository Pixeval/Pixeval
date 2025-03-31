// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Mako.Global.Enum;
using WinRT;
using WinUI3Utilities;
using NovelRankOptionExtension = Pixeval.Util.NovelRankOptionExtension;
using RankOptionExtension = Pixeval.Util.RankOptionExtension;

namespace Pixeval.Pages.Capability;

public sealed partial class RankingsPage : IScrollViewHost
{
    private static readonly IReadOnlyList<StringRepresentableItem> _IllustrationRankOption = RankOptionExtension.GetItems();

    private static readonly IReadOnlyList<StringRepresentableItem> _NovelRankOption = NovelRankOptionExtension.GetItems();

    public RankingsPage()
    {
        InitializeComponent();
        RankDateTimeCalendarDatePicker.Date = MaxDate;
        SimpleWorkTypeComboBox.SelectedEnum = App.AppViewModel.AppSettings.SimpleWorkType;
    }

    public DateTime MaxDate => DateTime.Now.AddDays(-2);

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
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
            RankOptionComboBox.ItemsSource = _IllustrationRankOption;
            RankOptionComboBox.SelectedItem = _IllustrationRankOption.First(r => Equals(r.Item, App.AppViewModel.AppSettings.IllustrationRankOption));
        }
        else
        {
            RankOptionComboBox.ItemsSource = _NovelRankOption;
            RankOptionComboBox.SelectedItem = _NovelRankOption.First(r => Equals(r.Item, App.AppViewModel.AppSettings.NovelRankOption));
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
