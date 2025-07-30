// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

namespace Pixeval.Controls;

/// <summary>
/// This view is provided as a holder for, normally a list of items with optionally incremental loading functionality
/// It features four additional properties in order to give a better UI experience
/// </summary>
[ContentProperty(Name = nameof(Content))]
public sealed partial class EntryView
{
    /// <summary>
    /// determines whether the list is empty, that is, not only it currently has no items presented, but also that no more item will be loaded. The correct image will be displayed to inform user when this property is <see langword="true"/>
    /// </summary>
    /// <remarks>
    /// <see cref="IsLoadingMore"/> takes higher priority than <see cref="HasNoItem"/>, i.e. If both of them are <see langword="true"/>, the effect of <see cref="IsLoadingMore"/> overwrites that of <see cref="HasNoItem"/>
    /// </remarks>
    [GeneratedDependencyProperty(DefaultValue = true)]
    public partial bool HasNoItem { get; set; }

    /// <summary>
    /// determines whether the list is under loading, the correct image will be displayed to inform user when this property is <see langword="true"/>
    /// </summary>
    /// <remarks>
    /// <see cref="IsLoadingMore"/> takes higher priority than <see cref="HasNoItem"/>, i.e. If both of them are <see langword="true"/>, the effect of <see cref="IsLoadingMore"/> overwrites that of <see cref="HasNoItem"/>
    /// </remarks>
    [GeneratedDependencyProperty(DefaultValue = false)]
    public partial bool IsLoadingMore { get; set; }

    [GeneratedDependencyProperty]
    public partial object? Content { get; set; }

    [GeneratedDependencyProperty]
    public partial string? TeachingTipTitle { get; set; }

    public EntryView() => InitializeComponent();

    partial void OnHasNoItemPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        HasNoItemStackPanel.Visibility = this is { HasNoItem: true, IsLoadingMore: false } ? Visibility.Visible : Visibility.Collapsed;
        SkeletonView.Visibility = this is { HasNoItem: true, IsLoadingMore: true } ? Visibility.Visible : Visibility.Collapsed;
    }

    partial void OnIsLoadingMorePropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        OnHasNoItemPropertyChanged(e);
    }
}
