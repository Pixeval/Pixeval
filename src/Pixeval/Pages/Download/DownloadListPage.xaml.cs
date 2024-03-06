#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/DownloadListPage.xaml.cs
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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.Controls.DialogContent;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.Pages.Download;

public sealed partial class DownloadListPage
{
    private bool _queriedBySuggestion;
    private readonly DownloadListPageViewModel _viewModel = new(App.AppViewModel.DownloadManager.QueuedTasks);

    public DownloadListPage() => InitializeComponent();

    private void ModeFilterComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _viewModel.ResetFilter();
    }

    private void PauseAllButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _viewModel.PauseSelectedItems();
    }

    private void ResumeAllButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _viewModel.ResumeSelectedItems();
    }

    private void CancelAllButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _viewModel.CancelSelectedItems();
    }

    private async void ClearDownloadListButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var dialogContent = new DownloadListPageDeleteTasksDialog();
        if (await this.CreateOkCancelAsync(
                DownloadListPageResources.DeleteDownloadHistoryRecordsFormatted.Format(_viewModel.SelectedEntries.Length),
                dialogContent) is ContentDialogResult.Primary)
        {
            if (dialogContent.DeleteLocalFiles)
                await Task.WhenAll(_viewModel.SelectedEntries
                    .Select(async t => await IoHelper.DeleteIllustrationTaskAsync(t.DownloadTask))
                    .ToArray());

            _viewModel.RemoveSelectedItems();
        }
    }

    private void FilterAutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        _viewModel.FilterTask(sender.Text);
    }

    private void FilterAutoSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        var selectedItem = (DownloadListEntryViewModel)args.SelectedItem;
        sender.Text = selectedItem.Title;
        _viewModel.CurrentOption = DownloadListOption.CustomSearch;
        _viewModel.ResetFilter([selectedItem]);
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
            _viewModel.CurrentOption = DownloadListOption.AllQueued;
            _viewModel.ResetFilter();
        }
        else
        {
            _viewModel.CurrentOption = DownloadListOption.CustomSearch;
            _viewModel.ResetFilter(_viewModel.FilteredTasks);
        }
    }

    private void SelectAllButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        AdvancedItemsView.SelectAll();
    }

    private void CancelSelectButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        AdvancedItemsView.DeselectAll();
    }

    private void ItemsView_OnSelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        _viewModel.SelectedEntries = sender.SelectedItems.Cast<DownloadListEntryViewModel>().ToArray();
    }

    private void DownloadListEntry_OnOpenIllustrationRequested(DownloadListEntry sender, DownloadListEntryViewModel viewModel)
    {
        viewModel.CreateWindowWithPage(_viewModel.DataProvider.Source);
    }

    private async void DownloadListEntry_OnViewModelChanged(DownloadListEntry sender, DownloadListEntryViewModel viewModel)
    {
        _ = await viewModel.TryLoadThumbnailAsync(_viewModel);
    }

    private void DownloadListPage_OnUnloaded(object sender, RoutedEventArgs e) => _viewModel.Dispose();
}
