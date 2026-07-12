using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Pixeval.Utilities;

namespace Pixeval.Controls;

public class TokenizingBoxPanel : Panel
{
    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<TokenizingBoxPanel, double>(nameof(ItemSpacing), 1);

    public static readonly StyledProperty<double> LineSpacingProperty =
        AvaloniaProperty.Register<TokenizingBoxPanel, double>(nameof(LineSpacing), 1);

    public static readonly StyledProperty<double> LineHeightProperty =
        AvaloniaProperty.Register<TokenizingBoxPanel, double>(nameof(LineHeight), double.NaN, validate: value => double.IsNaN(value) || value >= 0);

    public static readonly StyledProperty<double> MinInputWidthProperty =
        AvaloniaProperty.Register<TokenizingBoxPanel, double>(nameof(MinInputWidth), 120);

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<TokenizingBoxPanel, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<IDataTemplate?> TokenTemplateProperty =
        AvaloniaProperty.Register<TokenizingBoxPanel, IDataTemplate?>(nameof(TokenTemplate));

    public static readonly StyledProperty<Control?> InputElementProperty =
        AvaloniaProperty.Register<TokenizingBoxPanel, Control?>(nameof(InputElement));

    public static readonly StyledProperty<Control?> CountElementProperty =
        AvaloniaProperty.Register<TokenizingBoxPanel, Control?>(nameof(CountElement));

    static TokenizingBoxPanel()
    {
        AffectsMeasure<TokenizingBoxPanel>(
            ItemSpacingProperty,
            LineSpacingProperty,
            LineHeightProperty,
            MinInputWidthProperty,
            ItemsSourceProperty,
            TokenTemplateProperty,
            InputElementProperty,
            CountElementProperty);
        AffectsArrange<TokenizingBoxPanel>(
            ItemSpacingProperty,
            LineSpacingProperty,
            LineHeightProperty,
            MinInputWidthProperty,
            ItemsSourceProperty,
            TokenTemplateProperty,
            InputElementProperty,
            CountElementProperty);

        ItemsSourceProperty.Changed.AddClassHandler<TokenizingBoxPanel>((panel, _) => panel.RebuildChildren());
        TokenTemplateProperty.Changed.AddClassHandler<TokenizingBoxPanel>((panel, _) => panel.RebuildChildren());
        InputElementProperty.Changed.AddClassHandler<TokenizingBoxPanel>((panel, _) => panel.RebuildChildren());
        CountElementProperty.Changed.AddClassHandler<TokenizingBoxPanel>((panel, _) => panel.RebuildChildren());
    }

    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    public double LineSpacing
    {
        get => GetValue(LineSpacingProperty);
        set => SetValue(LineSpacingProperty, value);
    }

    public double LineHeight
    {
        get => GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }

    public double MinInputWidth
    {
        get => GetValue(MinInputWidthProperty);
        set => SetValue(MinInputWidthProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public IDataTemplate? TokenTemplate
    {
        get => GetValue(TokenTemplateProperty);
        set => SetValue(TokenTemplateProperty, value);
    }

    public Control? InputElement
    {
        get => GetValue(InputElementProperty);
        set => SetValue(InputElementProperty, value);
    }

    public Control? CountElement
    {
        get => GetValue(CountElementProperty);
        set => SetValue(CountElementProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var availableWidth = double.IsInfinity(availableSize.Width)
            ? double.PositiveInfinity
            : double.Max(0, availableSize.Width);
        var measureHeight = GetMeasureHeight(availableSize.Height);
        var input = InputElement;
        var count = CountElement;

        foreach (var child in Children)
        {
            var childWidth = ReferenceEquals(child, input) && !double.IsInfinity(availableWidth)
                ? availableWidth
                : double.PositiveInfinity;

            child.Measure(new Size(childWidth, measureHeight));
        }

        var x = 0d;
        var lineHeight = 0d;
        var totalHeight = 0d;
        var maxLineWidth = 0d;

        foreach (var child in Children)
        {
            if (ReferenceEquals(child, input) || ReferenceEquals(child, count))
                continue;

            if (!child.IsVisible)
                continue;

            AddMeasuredChild(child.DesiredSize.Width, child.DesiredSize.Height);
        }

        if (input?.IsVisible == true)
        {
            var countWidth = count?.IsVisible == true ? count.DesiredSize.Width : 0;
            var countSpacing = countWidth > 0 ? ItemSpacing : 0;
            var neededWidth = MinInputWidth + countSpacing + countWidth;

            if (!double.IsInfinity(availableWidth) && x > 0 && MathUtilities.GreaterThan(x + ItemSpacing + neededWidth, availableWidth))
                CommitLine();

            var prefixSpacing = x > 0 ? ItemSpacing : 0;
            var inputWidth = double.IsInfinity(availableWidth)
                ? double.Max(MinInputWidth, input.DesiredSize.Width)
                : double.Max(MinInputWidth, availableWidth - x - prefixSpacing - countSpacing - countWidth);
            var rowHeight = double.Max(input.DesiredSize.Height, count?.IsVisible == true ? count.DesiredSize.Height : 0);

            x += prefixSpacing + inputWidth;
            lineHeight = double.Max(lineHeight, GetLineHeight(rowHeight));

            if (count?.IsVisible == true)
                x += countSpacing + countWidth;
        }
        else if (count?.IsVisible == true)
        {
            AddMeasuredChild(count.DesiredSize.Width, count.DesiredSize.Height);
        }

        CommitLine();

        return new Size(double.IsInfinity(availableWidth) ? maxLineWidth : availableWidth, totalHeight);

        void AddMeasuredChild(double childWidth, double childHeight)
        {
            if (!double.IsInfinity(availableWidth) && x > 0 && MathUtilities.GreaterThan(x + ItemSpacing + childWidth, availableWidth))
                CommitLine();

            x += x > 0 ? ItemSpacing + childWidth : childWidth;
            lineHeight = double.Max(lineHeight, GetLineHeight(childHeight));
        }

        void CommitLine()
        {
            if (lineHeight <= 0)
                return;

            maxLineWidth = double.Max(maxLineWidth, x);
            totalHeight += totalHeight > 0 ? LineSpacing + lineHeight : lineHeight;
            x = 0;
            lineHeight = 0;
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var input = InputElement;
        var count = CountElement;
        var width = double.IsInfinity(finalSize.Width) ? double.PositiveInfinity : double.Max(0, finalSize.Width);
        var x = 0d;
        var y = 0d;
        var lineHeight = 0d;

        foreach (var child in Children)
        {
            if (ReferenceEquals(child, input) || ReferenceEquals(child, count))
                continue;

            if (!child.IsVisible)
                continue;

            ArrangeFixedChild(child);
        }

        if (input?.IsVisible == true)
        {
            var countWidth = count?.IsVisible == true ? count.DesiredSize.Width : 0;
            var countSpacing = countWidth > 0 ? ItemSpacing : 0;
            var neededWidth = MinInputWidth + countSpacing + countWidth;

            if (!double.IsInfinity(width) && x > 0 && MathUtilities.GreaterThan(x + ItemSpacing + neededWidth, width))
                MoveToNextLine();

            var inputX = x > 0 ? x + ItemSpacing : 0;
            var inputWidth = double.IsInfinity(width)
                ? double.Max(MinInputWidth, input.DesiredSize.Width)
                : double.Max(MinInputWidth, width - inputX - countSpacing - countWidth);
            var rowHeight = GetLineHeight(double.Max(input.DesiredSize.Height, count?.IsVisible == true ? count.DesiredSize.Height : 0));

            input.Arrange(new Rect(inputX, y, inputWidth, rowHeight));
            x = inputX + inputWidth;
            lineHeight = double.Max(lineHeight, rowHeight);

            if (count?.IsVisible == true)
            {
                var countX = x + countSpacing;
                count.Arrange(new Rect(countX, y, countWidth, rowHeight));
                x = countX + countWidth;
            }
        }
        else if (count?.IsVisible == true)
        {
            ArrangeFixedChild(count);
        }

        return finalSize;

        void ArrangeFixedChild(Control child)
        {
            var childSize = child.DesiredSize;
            if (!double.IsInfinity(width) && x > 0 && MathUtilities.GreaterThan(x + ItemSpacing + childSize.Width, width))
                MoveToNextLine();

            var childX = x > 0 ? x + ItemSpacing : 0;
            var childLineHeight = GetLineHeight(childSize.Height);
            child.Arrange(new Rect(childX, y, childSize.Width, childLineHeight));
            x = childX + childSize.Width;
            lineHeight = double.Max(lineHeight, childLineHeight);
        }

        void MoveToNextLine()
        {
            y += lineHeight + LineSpacing;
            x = 0;
            lineHeight = 0;
        }
    }

    private void RebuildChildren()
    {
        Children.Clear();

        if (ItemsSource is not null && TokenTemplate is { } tokenTemplate)
        {
            foreach (var item in ItemsSource)
            {
                var control = tokenTemplate.Build(item)
                              ?? throw new InvalidOperationException($"{nameof(TokenTemplate)} must build a control.");
                control.DataContext = item;
                Children.Add(control);
            }
        }

        if (InputElement is { } input)
            Children.Add(input);

        if (CountElement is { } count)
            Children.Add(count);
    }

    private double GetMeasureHeight(double availableHeight)
    {
        var lineHeight = LineHeight;
        return double.IsNaN(lineHeight) ? availableHeight : lineHeight;
    }

    private double GetLineHeight(double desiredHeight)
    {
        var lineHeight = LineHeight;
        return double.IsNaN(lineHeight) ? desiredHeight : lineHeight;
    }
}
