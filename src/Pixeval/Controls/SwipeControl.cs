using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;

namespace Pixeval.Controls;

public class SwipeControl : TransitioningContentControl
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(TransitioningContentControl);

    public event EventHandler<SwipeControl, SwipeControlSelectionChangedEventArgs>? SelectionChanged; 

    public static readonly StyledProperty<IList> ItemsSourceProperty = AvaloniaProperty.Register<SwipeControl, IList>(nameof(ItemsSource));

    public IList ItemsSource
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

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != SelectedIndexProperty && change.Property != ItemsSourceProperty)
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

        Content = item;

        SelectionChanged?.Invoke(this, new SwipeControlSelectionChangedEventArgs(SelectedIndex, item));
    }
}

public class SwipeControlSelectionChangedEventArgs(int newIndex, object? newItem) : EventArgs
{
    public int NewIndex { get; } = newIndex;

    public object? NewItem { get; } = newItem;
}
