// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Fonts;

namespace Pixeval.Utilities;

public static class AppBuilderExtensions
{
    private const string CjkFontFamilyName = "fonts:PixevalCjk#Noto Sans SC";

    private static readonly Uri _CjkFontCollectionKey = new("fonts:PixevalCjk", UriKind.Absolute);

    private static readonly Uri _CjkFontAssets = new("avares://Pixeval/Assets/Fonts", UriKind.Absolute);

    extension(AppBuilder builder)
    {
        public AppBuilder WithPixevalFonts() => builder
            .With(new FontManagerOptions
            {
                FontFallbacks =
                [
                    new FontFallback { FontFamily = new FontFamily(CjkFontFamilyName) }
                ]
            })
            .WithInterFont()
            .ConfigureFonts(static fontManager =>
                fontManager.AddFontCollection(new EmbeddedFontCollection(_CjkFontCollectionKey, _CjkFontAssets)));
    }
}
