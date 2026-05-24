// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Media;
using Pixeval.I18N;
using Pixeval.Models.Home;

namespace Pixeval.Views.Home;

public sealed partial class HomePageCardControl
{
    private const int PreviewItemCount = 6;

    private async void HomePageCardControl_OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Loaded -= HomePageCardControl_OnLoaded;

        try
        {
            var items = await HomePageCardSourceFactory.LoadPreviewItemsAsync(Card, PreviewItemCount);
            ShowPreviewItems(items);
        }
        catch (Exception)
        {
            ShowPlaceholder(I18NManager.GetResource(HomePageResources.CardPreviewFailedTextBlockText));
        }
    }

    private void ShowPreviewItems(IReadOnlyList<HomeCardPreviewItem> items)
    {
        if (items.Count is 0)
        {
            ShowPlaceholder(I18NManager.GetResource(HomePageResources.CardPreviewEmptyTextBlockText));
            return;
        }

        PreviewContent = Card.TemplateKind is HomePageCardTemplateKind.SingleImage or HomePageCardTemplateKind.SingleNovel or HomePageCardTemplateKind.SingleUser
            ? CreateSinglePreview(items[0])
            : CreateListPreview(items);
    }

    private Control CreateSinglePreview(HomeCardPreviewItem item) => new HomeCardSinglePreview(new(item, GetPreviewFallbackBrush(0)));

    private Control CreateListPreview(IReadOnlyList<HomeCardPreviewItem> items)
    {
        var columns = Card.ColumnSpan <= 1 ? 1 : 2;
        var rows = Math.Max(1, (int) Math.Ceiling(items.Count / (double) columns));
        var grid = new Grid
        {
            Margin = new(10),
            ColumnSpacing = 8,
            RowSpacing = 8
        };
        for (var column = 0; column < columns; column++)
            grid.ColumnDefinitions.Add(new(GridLength.Star));
        for (var row = 0; row < rows; row++)
            grid.RowDefinitions.Add(new(GridLength.Star));

        for (var i = 0; i < items.Count; i++)
        {
            var item = new HomeCardListPreviewItem(new(items[i], GetPreviewFallbackBrush(i)));
            Grid.SetColumn(item, i % columns);
            Grid.SetRow(item, i / columns);
            grid.Children.Add(item);
        }

        return grid;
    }

    private static IBrush GetPreviewFallbackBrush(int index)
    {
        Color[] colors =
        [
            Color.FromArgb(0xAA, 0x25, 0x8B, 0xC9),
            Color.FromArgb(0xAA, 0xD8, 0x7B, 0x43),
            Color.FromArgb(0xAA, 0x62, 0xB4, 0x6C),
            Color.FromArgb(0xAA, 0x8F, 0x72, 0xCC),
            Color.FromArgb(0xAA, 0xD9, 0x5C, 0x7A),
            Color.FromArgb(0xAA, 0x46, 0xA5, 0x9C)
        ];
        return new SolidColorBrush(colors[index % colors.Length]);
    }
}
