// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

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
