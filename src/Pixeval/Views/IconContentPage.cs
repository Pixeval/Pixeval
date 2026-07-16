// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Controls;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Pixeval.Utilities;

namespace Pixeval.Views;

public class IconContentPage : ContentPage
{
    protected IconContentPage()
    {
        var tuple = AvaloniaHelper.GetPageHeader(GetType());
        Header = tuple.Header;
        Icon = new SymbolIcon
        {
            Symbol = tuple.Symbol,
            FontSize = 16,
            IconVariant = IconVariant.Color
        };
    }
}

public class IconNavigationPage : NavigationPage
{
    protected IconNavigationPage()
    {
        var tuple = AvaloniaHelper.GetPageHeader(GetType());
        Header = tuple.Header;
        Icon = new SymbolIcon
        {
            Symbol = tuple.Symbol,
            FontSize = 16,
            IconVariant = IconVariant.Color
        };
    }
}
