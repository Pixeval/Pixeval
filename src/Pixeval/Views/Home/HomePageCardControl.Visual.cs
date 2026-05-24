// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using FluentIcons.Avalonia;
using FluentIcons.Common;

namespace Pixeval.Views.Home;

public sealed partial class HomePageCardControl
{
    private Grid CreateContent(bool isSelected)
    {
        var root = new Grid
        {
            ClipToBounds = false,
            Tag = this,
            Background = Brushes.Transparent
        };
        root.PointerPressed += Card_OnPointerPressed;
        root.PointerMoved += Card_OnPointerMoved;
        root.PointerReleased += Card_OnPointerReleased;
        root.PointerCaptureLost += Card_OnPointerCaptureLost;

        root.Children.Add(new Border
        {
            Background = CardTemplate.Brush,
            [!Border.BorderBrushProperty] = isSelected && IsEditing
                ? new DynamicResourceExtension("AccentFillColorDefaultBrush")
                : new DynamicResourceExtension("ControlStrokeColorDefaultBrush"),
            BorderThickness = new(2),
            CornerRadius = new(8),
            ClipToBounds = true,
            Child = CreateCardBody()
        });

        if (IsEditing)
            AddResizeHandles(root);

        return root;
    }

    private Grid CreateCardBody()
    {
        var cardGrid = new Grid
        {
            RowDefinitions =
            [
                new(GridLength.Star),
                new(GridLength.Auto)
            ]
        };

        cardGrid.Children.Add(new SymbolIcon
        {
            Symbol = CardTemplate.Symbol,
            IconVariant = IconVariant.Color,
            FontSize = 32,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Opacity = 0.92
        });

        var footer = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0x66, 0x00, 0x00, 0x00)),
            Padding = new(12, 8),
            Child = new StackPanel
            {
                Spacing = 2,
                Children =
                {
                    new TextBlock
                    {
                        Foreground = Brushes.White,
                        FontWeight = FontWeight.SemiBold,
                        Text = CardTemplate.Title,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    },
                    _sizeTextBlock
                }
            }
        };
        Grid.SetRow(footer, 1);
        cardGrid.Children.Add(footer);

        return cardGrid;
    }

    private TextBlock CreateSizeTextBlock() => new()
    {
        Foreground = Brushes.White,
        Opacity = 0.84,
        FontSize = 12,
        Text = BuildSizeText(),
        TextTrimming = TextTrimming.CharacterEllipsis
    };

    private void AddResizeHandles(Grid root)
    {
        AddResizeHandle(root, HomeCardEditAction.ResizeTopLeft, Avalonia.Layout.HorizontalAlignment.Left, Avalonia.Layout.VerticalAlignment.Top, 14, 14, 0, 0);
        AddResizeHandle(root, HomeCardEditAction.ResizeTop, Avalonia.Layout.HorizontalAlignment.Stretch, Avalonia.Layout.VerticalAlignment.Top, double.NaN, 10, 28, 0);
        AddResizeHandle(root, HomeCardEditAction.ResizeTopRight, Avalonia.Layout.HorizontalAlignment.Right, Avalonia.Layout.VerticalAlignment.Top, 14, 14, 0, 0);
        AddResizeHandle(root, HomeCardEditAction.ResizeRight, Avalonia.Layout.HorizontalAlignment.Right, Avalonia.Layout.VerticalAlignment.Stretch, 10, double.NaN, 0, 28);
        AddResizeHandle(root, HomeCardEditAction.ResizeBottomRight, Avalonia.Layout.HorizontalAlignment.Right, Avalonia.Layout.VerticalAlignment.Bottom, 14, 14, 0, 0);
        AddResizeHandle(root, HomeCardEditAction.ResizeBottom, Avalonia.Layout.HorizontalAlignment.Stretch, Avalonia.Layout.VerticalAlignment.Bottom, double.NaN, 10, 28, 0);
        AddResizeHandle(root, HomeCardEditAction.ResizeBottomLeft, Avalonia.Layout.HorizontalAlignment.Left, Avalonia.Layout.VerticalAlignment.Bottom, 14, 14, 0, 0);
        AddResizeHandle(root, HomeCardEditAction.ResizeLeft, Avalonia.Layout.HorizontalAlignment.Left, Avalonia.Layout.VerticalAlignment.Stretch, 10, double.NaN, 0, 28);
    }

    private static void AddResizeHandle(
        Grid root,
        HomeCardEditAction action,
        Avalonia.Layout.HorizontalAlignment horizontalAlignment,
        Avalonia.Layout.VerticalAlignment verticalAlignment,
        double width,
        double height,
        double horizontalMargin,
        double verticalMargin)
    {
        var handle = new Border
        {
            Width = width,
            Height = height,
            MinWidth = 10,
            MinHeight = 10,
            HorizontalAlignment = horizontalAlignment,
            VerticalAlignment = verticalAlignment,
            Margin = new(horizontalMargin, verticalMargin),
            Background = new SolidColorBrush(Color.FromArgb(0xD9, 0xFF, 0xFF, 0xFF)),
            [!Border.BorderBrushProperty] = new DynamicResourceExtension("AccentFillColorDefaultBrush"),
            BorderThickness = new(1),
            CornerRadius = new(5),
            Tag = action
        };
        handle.PointerPressed += ResizeHandle_OnPointerPressed;
        root.Children.Add(handle);
    }
}
