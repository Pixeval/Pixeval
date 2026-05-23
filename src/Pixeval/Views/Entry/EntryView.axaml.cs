// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using Pixeval.Controls;

namespace Pixeval.Views.Entry;

public class EntryView : TemplatedControl
{
    public static readonly StyledProperty<bool> HasNoItemProperty =
        AvaloniaProperty.Register<IconText, bool>(nameof(HasNoItem), true);

    public static readonly StyledProperty<bool> IsLoadingMoreProperty =
        AvaloniaProperty.Register<IconText, bool>(nameof(IsLoadingMore));

    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<IconText, object?>(nameof(Content));

    /// <summary>
    /// determines whether the list is empty, that is, not only it currently has no items presented, but also that no more item will be loaded. The correct image will be displayed to inform user when this property is <see langword="true"/>
    /// </summary>
    /// <remarks>
    /// <see cref="IsLoadingMore"/> takes higher priority than <see cref="HasNoItem"/>, i.e. If both of them are <see langword="true"/>, the effect of <see cref="IsLoadingMore"/> overwrites that of <see cref="HasNoItem"/>
    /// </remarks>
    public bool HasNoItem
    {
        get => GetValue(HasNoItemProperty);
        set => SetValue(HasNoItemProperty, value);
    }

    /// <summary>
    /// determines whether the list is under loading, the correct image will be displayed to inform user when this property is <see langword="true"/>
    /// </summary>
    /// <remarks>
    /// <see cref="IsLoadingMore"/> takes higher priority than <see cref="HasNoItem"/>, i.e. If both of them are <see langword="true"/>, the effect of <see cref="IsLoadingMore"/> overwrites that of <see cref="HasNoItem"/>
    /// </remarks>
    public bool IsLoadingMore
    {
        get => GetValue(IsLoadingMoreProperty);
        set => SetValue(IsLoadingMoreProperty, value);
    }

    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
}
