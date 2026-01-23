// Copyright (c) FluentAvalonia.
// Licensed under the GPL-3.0 License.

using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace FluentAvalonia.UI.Themes;

public class FluentAvaloniaTheme : Styles
{
    public FluentAvaloniaTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
