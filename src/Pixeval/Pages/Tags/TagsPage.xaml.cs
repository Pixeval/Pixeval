using Microsoft.UI.Xaml.Navigation;

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
}
