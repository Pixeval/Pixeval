// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Pixeval.Controls;

public class SwipeControl : TransitioningContentControl
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(TransitioningContentControl);

    public event EventHandler<Control, ImageViewerSelectionChangedEventArgs>? SelectionChanged; 

    public static readonly StyledProperty<IReadOnlyList<object>?> ItemsSourceProperty = AvaloniaProperty.Register<SwipeControl, IReadOnlyList<object>?>(nameof(ItemsSource));

    public IReadOnlyList<object>? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly StyledProperty<int> SelectedIndexProperty = AvaloniaProperty.Register<SwipeControl, int>(nameof(SelectedIndex));

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty = AvaloniaProperty.Register<SwipeControl, IDataTemplate?>(nameof(ItemTemplate));

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != SelectedIndexProperty && change.Property != ItemsSourceProperty && change.Property != ItemTemplateProperty)
            return;

        if (ItemsSource is not { } items || items.Count == 0 || SelectedIndex < 0 || SelectedIndex >= items.Count)
        {
            Content = null;
            return;
        }

        var item = items[SelectedIndex];

        if (change.Property == SelectedIndexProperty)
        {
            var oldItem = change.GetOldValue<int>();
            var newItem = change.GetNewValue<int>();
            IsTransitionReversed = oldItem > newItem;
        }

        if (ItemTemplate is null || !ItemTemplate.Match(item))
            Content = item;
        else
        {
            var content = ItemTemplate.Build(item);
            content?.DataContext = item;
            Content = content;
        }

        SelectionChanged?.Invoke(this, new ImageViewerSelectionChangedEventArgs(SelectedIndex, item));
    }
}

public class ImageViewerSelectionChangedEventArgs(int newIndex, object? newItem) : EventArgs
{
    public int NewIndex { get; } = newIndex;

    public object? NewItem { get; } = newItem;
}
