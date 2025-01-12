// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Controls;

public sealed class PixivNovelRtfParser(FrameworkElement frameworkElement) : PixivNovelParser<List<Paragraph>, ImageSource, DocumentViewerViewModel>
{
    private FrameworkElement FrameworkElement { get; } = frameworkElement;

    protected override List<Paragraph> Vector => [new()];

    protected override void AddLastSpan(List<Paragraph> result, string lastSpan)
    {
        var currentParagraph = result[^1];

        currentParagraph.Inlines.Add(new Run { Text = lastSpan });
    }

    protected override void AddRuby(List<Paragraph> result, string kanji, string ruby)
    {
        var currentParagraph = result[^1];

        currentParagraph.Inlines.Add(new Run
        {
            Text = kanji,
        });
        currentParagraph.Inlines.Add(new Run
        {
            Text = $"（{ruby}）",
            Foreground = new SolidColorBrush(Colors.Gray)
        });
    }

    protected override void AddHyperlink(List<Paragraph> result, string content, Uri uri)
    {
        var currentParagraph = result[^1];

        currentParagraph.Inlines.Add(new Hyperlink
        {
            NavigateUri = uri,
            Inlines = { new Run { Text = content } }
        });
    }

    protected override void AddInlineHyperlink(List<Paragraph> result, uint page, DocumentViewerViewModel viewModel)
    {
        var currentParagraph = result[^1];

        var hyperlink = new Hyperlink { Inlines = { new Run { Text = MiscResources.GoToPageFormatted.Format(page) } } };
        hyperlink.Click += (_, _) => viewModel.JumpToPageRequested?.Invoke((int)page - 1);
        currentParagraph.Inlines.Add(hyperlink);
    }

    protected override void AddChapter(List<Paragraph> result, string chapterText)
    {
        var currentParagraph = result[^1];

        currentParagraph.Inlines.Add(new LineBreak());
        currentParagraph.Inlines.Add(new Run { Text = chapterText, FontSize = 20, FontWeight = FontWeights.Bold });
        currentParagraph.Inlines.Add(new LineBreak());
    }

    protected override void AddUploadedImage(List<Paragraph> result, DocumentViewerViewModel viewModel, long imageId)
    {
        var image = new LazyImage { Stretch = Stretch.Uniform };
        result.Add(new()
        {
            Inlines = { new InlineUIContainer { Child = image } },
            TextAlignment = TextAlignment.Center
        });
        result.Add(new());
        viewModel.PropertyChanged += (s, e) =>
        {
            var vm = s.To<DocumentViewerViewModel>();
            if (e.PropertyName == nameof(vm.UploadedImages) + imageId)
                image.Source = vm.UploadedImages[imageId];
        };
    }

    protected override void AddPixivImage(List<Paragraph> result, DocumentViewerViewModel viewModel, long imageId, int page)
    {
        var key = (imageId, page);

        var image = new LazyImage { Stretch = Stretch.Uniform };
        var button = new Button { Content = image, Padding = new(5) };
        button.Click += async (_, _) => await FrameworkElement.CreateIllustrationPageAsync(imageId, viewModel.IllustrationLookup.Keys.Select(t => t.Item1).ToArray());
        var info = viewModel.IllustrationLookup[key];
        ToolTipService.SetToolTip(button, $"{info.Id}: {info.Illust.Title}");
        result.Add(new()
        {
            Inlines = { new InlineUIContainer { Child = button } },
            TextAlignment = TextAlignment.Center
        });
        result.Add(new());
        viewModel.PropertyChanged += (s, e) =>
        {
            var vm = s.To<DocumentViewerViewModel>();
            if (e.PropertyName == nameof(vm.IllustrationImages) + key.GetHashCode())
                image.Source = vm.IllustrationImages[key];
        };
    }
}
