using System;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;

namespace Pixeval.Themes;

public sealed class PixevalFluentTheme : FluentTheme
{
    public PixevalFluentTheme()
    {
        // The token editor embeds a compact TextBox inside its own decoration.
        // Retain that template before Material supplies the application's default TextBox theme.
        if (!TryGetResource(typeof(TextBox), null, out var resource) || resource is not ControlTheme theme)
            throw new InvalidOperationException("The Fluent theme does not contain a TextBox theme.");

        Resources["PixevalFluentTextBoxTheme"] = theme;
    }
}
