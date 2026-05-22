// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Linq;

namespace Pixeval.Views.Settings;

public record DependencyViewModel(string Name, string Author, string Url, string License)
{
    // ReSharper disable StringLiteralTypo
    private static readonly (string, string)[] _Dependencies =
    [
        ("CommunityToolkit/dotnet", LicenseTexts.MIT("2021", ".NET Foundation and Contributors.")),
        ("davidxuang/FluentIcons", LicenseTexts.MIT("2022", "davidxuang")),
        ("dotnetcore/WebApiClient", LicenseTexts.MIT("2020", ".NET Core Community")),
        ("whistyun/Markdown.Avalonia", LicenseTexts.MIT("2020", "Whistyun")),
        ("praeclarum/sqlite-net", LicenseTexts.MIT("2019", "Krueger Systems, Inc.")),
        ("zxbmmmmmmmmm/SmoothScroll.Avalonia", LicenseTexts.MIT("2026", "zxbmmmmmmmmm")),
        ("wieslawsoltes/Svg.Skia", LicenseTexts.MIT("2020", "Wiesław Šoltés")),
        ("irihitech/Ursa.Avalonia", LicenseTexts.MIT("2025", ".NET Foundation and Contributors")),
        ("irihitech/Semi.Avalonia", LicenseTexts.MIT("2022", "IRIHI Technology"))
    ];
    // ReSharper restore StringLiteralTypo

    public static IEnumerable<DependencyViewModel> DependencyViewModels =>
        _Dependencies.Select(t =>
        {
            var segments = t.Item1.Split('/');
            return new DependencyViewModel(segments[^1], segments[0], "https://github.com/" + t.Item1, t.Item2);
        });
}
