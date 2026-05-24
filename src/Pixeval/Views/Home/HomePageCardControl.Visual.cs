// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Views.Home;

public sealed partial class HomePageCardControl
{
    private void ShowPlaceholder(string text)
    {
        PreviewContent = null;
        PlaceholderText = text;
    }

    private void ApplyCardTemplate(HomeCardTemplate template)
    {
        Background = template.Brush;
        CardTitle = template.Title;
        CardSymbol = template.Symbol;
    }
}
