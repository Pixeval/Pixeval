#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/DownloadPage.xaml.cs
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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Controls;
using Pixeval.Controls.DialogContent;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.Pages.Download;

public sealed partial class DownloadPage
{
    private bool _queriedBySuggestion;

    public DownloadPage() => InitializeComponent();

    private void ModeFilterComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        DownloadView.ViewModel.ResetFilter();
    }

    private void PauseAllButton_OnClicked(object sender, RoutedEventArgs e)
    {
        DownloadView.ViewModel.PauseSelectedItems();
    }

    private void ResumeAllButton_OnClicked(object sender, RoutedEventArgs e)
    {
        DownloadView.ViewModel.ResumeSelectedItems();
    }

    private void CancelAllButton_OnClicked(object sender, RoutedEventArgs e)
    {
        DownloadView.ViewModel.CancelSelectedItems();
    }

    private async void ClearDownloadListButton_OnClicked(object sender, RoutedEventArgs e)
    {
        var dialogContent = new DownloadPageDeleteTasksDialog();
        if (await this.CreateOkCancelAsync(
                DownloadPageResources.DeleteDownloadHistoryRecordsFormatted.Format(DownloadView.ViewModel.SelectedEntries.Length),
                dialogContent) is ContentDialogResult.Primary)
        {
            if (dialogContent.DeleteLocalFiles)
                foreach (var entry in DownloadView.ViewModel.SelectedEntries)
                    entry.DownloadTask.Delete();

            DownloadView.ViewModel.RemoveSelectedItems();
        }
    }

    private void FilterAutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        DownloadView.ViewModel.FilterTask(sender.Text);
    }

    private void FilterAutoSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        var selectedItem = (DownloadItemViewModel)args.SelectedItem;
        sender.Text = selectedItem.Entry.Title;
        DownloadView.ViewModel.CurrentOption = DownloadListOption.CustomSearch;
        DownloadView.ViewModel.ResetFilter([selectedItem]);
        _queriedBySuggestion = true;
    }

    private void FilterAutoSuggestBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (_queriedBySuggestion)
        {
            _queriedBySuggestion = false;
            return;
        }

        if (sender.Text.IsNullOrBlank())
        {
            DownloadView.ViewModel.CurrentOption = DownloadListOption.AllQueued;
            DownloadView.ViewModel.ResetFilter();
        }
        else
        {
            DownloadView.ViewModel.CurrentOption = DownloadListOption.CustomSearch;
            DownloadView.ViewModel.ResetFilter(DownloadView.ViewModel.FilteredTasks);
        }
    }

    private void SelectAllButton_OnClicked(object sender, RoutedEventArgs e)
    {
        DownloadView.ItemsView.SelectAll();
    }

    private void CancelSelectButton_OnClicked(object sender, RoutedEventArgs e)
    {
        DownloadView.ItemsView.DeselectAll();
    }
}
