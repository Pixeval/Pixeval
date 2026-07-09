// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako;
using Mako.Global.Enum;
using Pixeval.Controls;
using Pixeval.ViewModels;

namespace Pixeval.Views.Capability;

public partial class WorkRankingPage : IconContentPage
{
    public WorkRankingPage() : this(PixevalSettings.SimpleWorkType, App.AppViewModel.AppSettings.SearchSettings.IllustrationRankOption, MaxDate)
    {
    }

    public WorkRankingPage(SimpleWorkType simpleWorkType, RankOption rankOption, DateTime rankingDate, IWorkViewViewModel? viewModel = null)
    {
        InitializeComponent();
        SimpleWorkTypeComboBox.SelectedValue = simpleWorkType;
        ChangeEnumSource();
        RankOptionComboBox.SelectedValue = rankOption;
        RankDateTimeCalendarDatePicker.SelectedDate = rankingDate;
        if (viewModel is not null)
            WorkContainer.SetViewModel(viewModel);
        else
            ChangeSource();
    }

    public static DateTime MaxDate => MakoClient.RankingMaxDateTime.LocalDateTime;

    private void SimpleWorkTypeComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    {
        var oldRankOption = RankOptionComboBox.SelectedValue;
        ChangeEnumSource();
        if (Equals(oldRankOption, RankOptionComboBox.SelectedValue))
            ChangeSource();
    }

    private void ChangeEnumSource()
    {
        var selectedWorkType = SimpleWorkTypeComboBox.GetSelectedValue<SimpleWorkType>();
        RankOptionComboBox.ItemsSource = SymbolComboBoxItem.GetValues<RankOption>(selectedWorkType);
        RankOptionComboBox.SelectedValue = selectedWorkType is SimpleWorkType.Illustration
            ? App.AppViewModel.AppSettings.SearchSettings.IllustrationRankOption
            : App.AppViewModel.AppSettings.SearchSettings.NovelRankOption;
    }

    private void RankOptionComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    {
        if (RankOptionComboBox.SelectedValue is not null)
            ChangeSource();
    }

    private void WorkContainer_OnRefreshRequested(object? sender, RoutedEventArgs e)
    {
        ChangeSource();
    }

    private void CalendarDatePicker_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
            return;
        // 傻逼bug see also https://github.com/AvaloniaUI/Avalonia/issues/18418
        // ChangeSource();
    }

    private void ChangeSource()
    {
        WorkContainer.ResetEngine(App.AppViewModel.MakoClient.WorkRanking(
            SimpleWorkTypeComboBox.GetSelectedValue<SimpleWorkType>(),
            RankOptionComboBox.GetSelectedValue<RankOption>(),
            new(RankDateTimeCalendarDatePicker.SelectedDate ?? MaxDate)));
    }
}
