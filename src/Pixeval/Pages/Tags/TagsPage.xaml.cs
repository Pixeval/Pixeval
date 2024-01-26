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
}
