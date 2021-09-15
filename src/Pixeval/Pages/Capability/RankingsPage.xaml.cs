using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using Microsoft.UI.Xaml;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Events;
using Pixeval.Misc;
using Pixeval.Options;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Util.Generic;

namespace Pixeval.Pages.Capability
{
    public sealed partial class RankingsPage : ISortedIllustrationContainerPageHelper
    {
        public IllustrationContainer ViewModelProvider => IllustrationContainer;

        public SortOptionComboBox SortOptionProvider => SortOptionComboBox;

        public RankingsPage()
        {
            InitializeComponent();
        }

        public override void Dispose(NavigatingCancelEventArgs navigatingCancelEventArgs)
        {
            IllustrationContainer.ViewModel.Dispose();
        }

        public override void Prepare(NavigationEventArgs navigationEventArgs)
        {
            SortOptionComboBox.SelectedItem = MakoHelper.GetAppSettingDefaultSortOptionWrapper();
            RankOptionComboBox.SelectedItem = RankOptionWrapper.AvailableOptions().Of(RankOption.Day);
            RankDateTimeCalendarDatePicker.Date = DateTime.Now.AddDays(-2);
            EventChannel.Default.Subscribe<MainPageFrameNavigatingEvent>(() => IllustrationContainer.ViewModel.FetchEngine?.Cancel());
        }

        private void RankingsPage_OnLoaded(object? sender, RoutedEventArgs e)
        {
            if (App.Window.GetNavigationModeAndReset() is not NavigationMode.Back)
            {
                ChangeSource();
            }
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
            ((ISortedIllustrationContainerPageHelper) this).OnSortOptionChanged();
        }

        private void ChangeSource()
        {
            var rankOption = (RankOptionComboBox.SelectedItem as RankOptionWrapper)?.Value ?? RankOption.Day;
            var dateTime = RankDateTimeCalendarDatePicker.Date?.DateTime ?? DateTime.Now.AddDays(-2);
            _ = IllustrationContainer.ViewModel.ResetAndFillAsync(App.MakoClient.Ranking(rankOption, dateTime));
        }
    }
}
