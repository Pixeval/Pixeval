// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIcon = FluentIcons.WinUI.SymbolIcon;
using SymbolIconSource = FluentIcons.WinUI.SymbolIconSource;

namespace Pixeval.Controls.Windowing;

public interface INavigationViewItem;

public class NavigationViewSeparator : INavigationViewItem;

public partial class NavigationViewTag(string header, Type navigateTo, object? parameter) : ObservableObject, INavigationViewItem
{
    public Type NavigateTo { get; set; } = navigateTo;

    public object? Parameter { get; set; } = parameter;

    [ObservableProperty]
    public partial string Header { get; set; } = header;

    public IconSource? IconSource =>
        Symbol is null
            ? ImageUri is null
                ? null
                : new ImageIconSource { ImageSource = new BitmapImage(ImageUri) }
            : new SymbolIconSource
            {
                Symbol = Symbol.Value,
                IconVariant = IconVariant.Color
            };

    public IconElement? Icon =>
        Symbol is null
            ? ImageUri is null
                ? null
                : new ImageIcon { Source = new BitmapImage(ImageUri) }
            : new SymbolIcon
            {
                Symbol = Symbol.Value,
                IconVariant = IconVariant.Color 
            };

    [ObservableProperty]
    public partial bool ShowIconBadge { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Icon))]
    [NotifyPropertyChangedFor(nameof(IconSource))]
    public partial Symbol? Symbol { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Icon))]
    [NotifyPropertyChangedFor(nameof(IconSource))]
    public partial Uri? ImageUri { get; set; }

    public NavigationTransitionInfo? TransitionInfo { get; set; }

    /// <inheritdoc />
    public override string ToString() => Header;
}

public partial class NavigationViewTag<TPage>(string header, object? parameter = null) : NavigationViewTag(header, typeof(TPage), parameter);

public sealed partial class NavigationViewTag<TPage, TParam>(string header, TParam parameter) : NavigationViewTag(header, typeof(TPage), parameter)
{
    public new TParam Parameter
    {
        get => (TParam) base.Parameter!;
        set => base.Parameter = value;
    }
}
