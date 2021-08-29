using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using Mako.Global.Enum;
using Pixeval.Events;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pixeval.Pages.Capability
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RankingsPage
    {
        public RankingsPage()
        {
            InitializeComponent();
        }


        private DateTime MaxDate => DateTime.Now.AddDays(-2);

        public override void Dispose(NavigatingCancelEventArgs navigatingCancelEventArgs)
        {
            IllustrationGrid.ViewModel.Dispose();
        }

        public override void Prepare(NavigationEventArgs navigationEventArgs)
        {
            RankOptionComboBox.SelectedItem = LocalizedBoxHelper.Of<RankOption, RankOptionWrapper>(RankOption.Day);
            RankDateTimeCalendarDatePicker.Date = DateTime.Now.AddDays(-2);
            EventChannel.Default.Subscribe<MainPageFrameNavigatingEvent>(() => IllustrationGrid.ViewModel.FetchEngine?.Cancel());
        }

        private async void RankingsPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (MainWindow.GetNavigationModeAndReset() is not NavigationMode.Back)
            {
                await ChangeSource();
            }

            IllustrationGrid.Focus(FocusState.Programmatic);
        }

        private async void RankOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await ChangeSource();
        }
        
        private async void RankDateTimeCalendarDatePicker_OnDateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            await ChangeSource();
        }

        private async Task ChangeSource()
        {
            // TODO: Something wrong. Need fix.
            if (TryGetRankOption(RankOptionComboBox, out var rankOption) && TryGetDatetime(RankDateTimeCalendarDatePicker, out var dateTime))
            {
                await IllustrationGrid.ViewModel.ResetAndFill(App.MakoClient.Ranking(rankOption, dateTime));
            }
        }
        #region Helper Functions

        private bool TryGetDatetime(CalendarDatePicker sender, out DateTime dateTime)
        {
            if (sender is {Date: { } })
            {
                dateTime = sender.Date.Value.DateTime;
                return true;
            }

            dateTime = DateTime.Now;
            return false;
        }

        private bool TryGetRankOption(ComboBox sender, out RankOption option)
        {
            if (sender is {SelectedItem: RankOptionWrapper {Value: var t}})
            {
                option = t;
                return true;
            }

            option = RankOption.Day;
            return false;
        }
        
        #endregion
    }
}
