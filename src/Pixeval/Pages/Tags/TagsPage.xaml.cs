using System;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Util.UI;

namespace Pixeval.Pages.Tags;

public sealed partial class TagsPage
{
    private readonly TagsPageViewModel _viewModel = new();

    public TagsPage()
    {
        InitializeComponent();
    }

    public override void OnPageDeactivated(NavigatingCancelEventArgs e) => _viewModel.Dispose();

    private void TagsEntry_OnTagTapped(TagsEntry sender, string tag)
    {
        if (!_viewModel.SelectedTags.Contains(tag))
            _viewModel.SelectedTags.Add(tag);
    }

    private void TagsEntry_OnFileDeleted(TagsEntry sender, TagsEntryViewModel viewModel)
    {
        _ = _viewModel.DataProvider.View.Remove(viewModel);
    }

    private async void ChangeWorkingPath_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (await Window.OpenFolderPickerAsync() is { } folder) 
            _viewModel.WorkingDirectory = folder.Path;
    }
}
