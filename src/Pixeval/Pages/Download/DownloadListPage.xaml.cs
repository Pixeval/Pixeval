#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/DownloadListPage.xaml.cs
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Download;
using Pixeval.Utilities;

namespace Pixeval.Pages.Download
{
    public sealed partial class DownloadListPage
    {
        private DownloadListPageViewModel _viewModel = null!;

        public DownloadListPage()
        {
            InitializeComponent();
        }

        public override void OnPageActivated(NavigationEventArgs e)
        {
            _viewModel = new DownloadListPageViewModel(((IEnumerable<ObservableDownloadTask>) e.Parameter).Select(o => new DownloadListEntryViewModel(o)).ToList());
        }

        public override void OnPageDeactivated(NavigatingCancelEventArgs e)
        {
            foreach (var downloadListEntryViewModel in _viewModel.DownloadTasks)
            {
                downloadListEntryViewModel.Dispose();
            }
        }

        private void ModeFilterComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.ResetFilter();
        }

        private void PauseAllButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _viewModel.PauseAll();
        }

        private void ResumeAllButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _viewModel.ResumeAll();
        }

        private void CancelAllButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _viewModel.CancelAll();
        }

        private void ClearDownloadListButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _viewModel.ClearDownloadList(); // TODO
        }

        private void FilterAutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (sender.Text.IsNotNullOrBlank())
                _viewModel.FilterTask(sender.Text);
            else
            {
                _viewModel.ResetFilter();
                _viewModel.CurrentOption = DownloadListOption.AllQueued;
            }
        }

        private bool _queriedBySuggestion;

        private void FilterAutoSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = ((IDownloadTask)args.SelectedItem).Title;
            _viewModel.CurrentOption = DownloadListOption.CustomSearch;
            _viewModel.ResetFilter(Enumerates.EnumerableOf((IDownloadTask) args.SelectedItem));
            _queriedBySuggestion = true;
        }

        private void FilterAutoSuggestBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (_queriedBySuggestion)
            {
                _queriedBySuggestion = false;
                return;
            }
            _viewModel.CurrentOption = DownloadListOption.CustomSearch;
            _viewModel.ResetFilter(_viewModel.FilteredTasks);
        }

        // Remarks: The VisualStateManager won't work I really don't know why
        private void DownloadListPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var greaterThan1000 = e.NewSize.Width > 1000;
            //Grid.SetRowSpan(FunctionBarTitleTextBlock, greaterThan1000 ? 2 : 1);
            //Grid.SetRow(FunctionBar, greaterThan1000 ? 2 : 3);
            //Grid.SetRowSpan(FunctionBar, greaterThan1000 ? 2 : 1);
            //FunctionBar.HorizontalAlignment = greaterThan1000 ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            //FunctionBar.Margin = new Thickness(0, greaterThan1000 ? 0 : 10, 0, 0);
        }

        private void DownloadListEntry_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is DownloadListEntry entry)
            {
                entry.ViewModel.Selected = !entry.ViewModel.Selected;
                if (entry.ViewModel.Selected)
                    _viewModel.SelectedTasks.Add(entry.ViewModel);
                else
                    _viewModel.SelectedTasks.Remove(entry.ViewModel);
                _viewModel.UpdateSelection();
            }
        }

        private void SelectAllButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _viewModel.SelectedTasks.Clear();
            _viewModel.SelectedTasks.AddRange(_viewModel.DownloadTasksView.Select(x => ((DownloadListEntryViewModel) x).DownloadTask));
            _viewModel.SelectedTasks.ForEach(x => x.Selected = true);
            _viewModel.UpdateSelection();
        }

        private void CancelSelectButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _viewModel.SelectedTasks.ForEach(x => x.Selected = false);
            _viewModel.SelectedTasks.Clear();
            _viewModel.UpdateSelection();
        }
    }
}