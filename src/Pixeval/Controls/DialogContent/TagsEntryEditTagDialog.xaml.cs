using System.Collections.ObjectModel;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Pages.Tags;

namespace Pixeval.Controls.DialogContent;

public sealed partial class TagsEntryEditTagDialog : UserControl
{
    private readonly TagsEntryViewModel _viewModel;

    public ObservableCollection<string> Tags { get; }

    public TagsEntryEditTagDialog(TagsEntryViewModel viewModel)
    {
        _viewModel = viewModel;
        Tags = [.. viewModel.TagsSet!];
        InitializeComponent();
    }

    private void TokenizingTextBox_OnTokenItemAdding(TokenizingTextBox sender, TokenItemAddingEventArgs e)
    {
        if (Tags.Contains(e.TokenText))
            e.Cancel = true;
    }
}
