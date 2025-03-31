// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Microsoft.UI.Xaml;
using Pixeval.Util.UI;

namespace Pixeval.Pages.Tags;

public sealed partial class TagsPage
{
    private readonly TagsPageViewModel _viewModel = new();

    public TagsPage() => InitializeComponent();

    private void TagsEntry_OnTagClick(TagsEntry sender, string tag)
    {
        if (!_viewModel.SelectedTags.Contains(tag))
            _viewModel.SelectedTags.Add(tag);
    }

    private void TagsEntry_OnFileDeleted(TagsEntry sender, TagsEntryViewModel viewModel)
    {
        _ = _viewModel.DataProvider.View.Remove(viewModel);
    }

    private async void ChangeWorkingPath_OnClicked(object sender, RoutedEventArgs e)
    {
        if (await this.OpenFolderPickerAsync() is { } folder)
            _viewModel.WorkingDirectory = folder.Path;
    }

    public override void CompleteDisposal()
    {
        base.CompleteDisposal();
        _viewModel.Dispose();
    }
}
