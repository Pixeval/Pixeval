// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;

namespace Pixeval.Pages.Misc;

public record DependencyViewModel(string Name, string Author, string Url, string License)
{
    // ReSharper disable StringLiteralTypo
    private static readonly (string, string)[] _Dependencies =
    [
        ("CommunityToolkit", LicenseTexts.MIT("2024", ".NET Foundation and Contributors.")),
        ("mysticmind/reversemarkdown-net", LicenseTexts.MIT("2015", "Babu Annamalai")),
        ("Poker-sang/WinUI3Utilities", LicenseTexts.MIT("2023", "Poker-sang")),
        ("codebude/QRCoder", LicenseTexts.MIT("2013-2018", "Raffael Herrmann")),
        ("Sergio0694/PolySharp", LicenseTexts.MIT("2022", "Sergio Pedri")),
        ("SixLabors/ImageSharp", LicenseTexts.ImageSharp),
        ("mbdavid/LiteDB", LicenseTexts.MIT("2014-2022", "Mauricio David")),
        ("davidxuang/FluentIcons", LicenseTexts.MIT("2022", "davidxuang")),
        ("LasmGratel/PininSharp", LicenseTexts.MIT("2021", "Lasm Gratel")),
        ("QuestPDF/QuestPDF", LicenseTexts.QuestPDF),
        ("dotnetcore/WebApiClient", LicenseTexts.MIT("2020", ".NET Core Community"))
    ];
    // ReSharper restore StringLiteralTypo

    public static IEnumerable<DependencyViewModel> DependencyViewModels =>
        _Dependencies.Select(t =>
        {
            var segments = t.Item1.Split('/');
            return new DependencyViewModel(segments[^1], segments[0], "https://github.com/" + t.Item1, t.Item2);
        });
}
