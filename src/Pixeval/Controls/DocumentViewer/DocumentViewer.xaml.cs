// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Documents;
using Pixeval.Utilities;

namespace Pixeval.Controls;

public sealed partial class DocumentViewer
{
    public DocumentViewer() => InitializeComponent();

    async partial void OnNovelItemPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (NovelItem is null)
        {
            ViewModel = null;
            return;
        }

        try
        {
            LoadSuccessfully = false;
            IsLoading = true;
            ViewModel = await DocumentViewerViewModel.CreateAsync(this, NovelItem, _ => LoadSuccessfully = true);
            ViewModel.JumpToPageRequested += newPage => CurrentPage = newPage;
            ViewModel.Pages.CollectionChanged += (_, _) => PageCount = ViewModel.Pages.Count;
            PageCount = ViewModel.Pages.Count;
            if (CurrentPage is 0)
                UpdateContent();
            else
                CurrentPage = 0;
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnCurrentPagePropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (NovelItem is null)
            return;

        UpdateContent();
    }

    partial void OnPageCountPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        IsMultiPage = PageCount > 1;
    }

    private void UpdateContent()
    {
        NovelRichTextBlock.Blocks.Clear();
        if (CurrentParagraph is { } current)
            NovelRichTextBlock.Blocks.AddRange(current);
    }

    private List<Paragraph>? CurrentParagraph => ViewModel is not null && CurrentPage < PageCount ? ViewModel.Pages[CurrentPage] : null;
}
