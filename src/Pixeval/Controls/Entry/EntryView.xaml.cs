// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

/// <summary>
/// This view is provided as a holder for, normally a list of items with optionally incremental loading functionality
/// It features four additional properties in order to give a better UI experience
/// </summary>
/// 
/// <remarks>
/// <list type="bullet">
///     <item>
///         <see cref="HasNoItem"/> determines whether the list is empty, that is, not only it currently has no items presented, but also that no more item will be loaded. The correct image will be displayed to inform user when this property is <c>true</c>
///     </item>
///     <item>
///         <see cref="IsLoadingMore"/> determines whether the list is under loading, the correct image will be displayed to inform user when this property is <c>true</c>
///     </item>
///     <item>
///         <see cref="IsLoadingMore"/> takes higher priority than <see cref="HasNoItem"/>, i.e. If both of them are <c>true</c>, the effect of <see cref="IsLoadingMore"/> overwrites that of <see cref="HasNoItem"/>
///     </item>
/// </list>
/// </remarks>
[DependencyProperty<bool>("HasNoItem", "true", nameof(OnHasNoItemChanged))]
[DependencyProperty<bool>("IsLoadingMore", "false", nameof(OnHasNoItemChanged))]
[DependencyProperty<object>("Content")]
[DependencyProperty<string>("TeachingTipTitle")]
public sealed partial class EntryView
{
    public EntryView() => InitializeComponent();

    private static void OnHasNoItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (EntryView)d;
        control.HasNoItemStackPanel.Visibility = control is { HasNoItem: true, IsLoadingMore: false } ? Visibility.Visible : Visibility.Collapsed;
        control.SkeletonView.Visibility = control is { HasNoItem: true, IsLoadingMore: true } ? Visibility.Visible : Visibility.Collapsed;
    }
}
