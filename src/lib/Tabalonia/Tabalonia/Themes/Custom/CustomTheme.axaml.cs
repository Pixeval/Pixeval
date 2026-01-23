using Avalonia.Markup.Xaml;

namespace Tabalonia.Themes.Custom;

public class CustomTheme : Styles
{
    public CustomTheme(IServiceProvider? sp = null)
    {
        AvaloniaXamlLoader.Load(sp, this);
    }
}