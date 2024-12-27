using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Documents;
using Pixeval.Utilities;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<NovelItemViewModel>("NovelItem", DependencyPropertyDefaultValue.UnsetValue, nameof(OnNovelItemChanged), IsNullable = true)]
[DependencyProperty<double>("NovelMaxWidth", "1000d")]
[DependencyProperty<double>("LineHeight", "28d")]
[DependencyProperty<int>("CurrentPage", "0", nameof(OnCurrentPageChanged))]
[DependencyProperty<int>("PageCount", "0", nameof(OnPageCountChanged))]
[DependencyProperty<bool>("IsMultiPage", "false")]
[DependencyProperty<bool>("LoadSuccessfully", "true")]
[DependencyProperty<bool>("IsLoading", "false")]
[DependencyProperty<DocumentViewerViewModel>("ViewModel", IsNullable = true)]
public sealed partial class DocumentViewer
{
    public DocumentViewer() => InitializeComponent();

    public static async void OnNovelItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var viewer = d.To<DocumentViewer>();
        if (viewer.NovelItem is null)
        {
            viewer.ViewModel = null;
            return;
        }

        try
        {
            viewer.LoadSuccessfully = false;
            viewer.IsLoading = true;
            viewer.ViewModel = await DocumentViewerViewModel.CreateAsync(viewer.NovelItem, _ => viewer.LoadSuccessfully = true);
            viewer.ViewModel.JumpToPageRequested += newPage => viewer.CurrentPage = newPage;
            viewer.ViewModel.Pages.CollectionChanged += (_, _) => viewer.PageCount = viewer.ViewModel.Pages.Count;
            viewer.PageCount = viewer.ViewModel.Pages.Count;
            if (viewer.CurrentPage is 0)
                viewer.UpdateContent();
            else
                viewer.CurrentPage = 0;
        }
        finally
        {
            viewer.IsLoading = false;
        }
    }

    public static void OnCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var viewer = d.To<DocumentViewer>();
        if (viewer.NovelItem is null)
            return;

        viewer.UpdateContent();
    }

    public static void OnPageCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var viewer = d.To<DocumentViewer>();
        viewer.IsMultiPage = viewer.PageCount > 1;
    }

    private void UpdateContent()
    {
        NovelRichTextBlock.Blocks.Clear();
        if (CurrentParagraph is { } current)
            NovelRichTextBlock.Blocks.AddRange(current);
    }

    private List<Paragraph>? CurrentParagraph => ViewModel is not null && CurrentPage < PageCount ? ViewModel.Pages[CurrentPage] : null;
}
