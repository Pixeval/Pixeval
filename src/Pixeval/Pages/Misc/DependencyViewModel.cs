using System.Collections.Generic;
using System.Linq;

namespace Pixeval.Pages.Misc;

public record DependencyViewModel(string Name, string Author, string Url, string License)
{
    // ReSharper disable StringLiteralTypo
    private static readonly (string, string)[] _dependencies =
    [
        ("CommunityToolkit", LicenseTexts.MIT("2024", ".NET Foundation and Contributors.")),
        ("mysticmind/reversemarkdown-net", LicenseTexts.MIT("2015", "Babu Annamalai")),
        ("GitTools/GitVersion", LicenseTexts.MIT("2021", "NServiceBus Ltd, GitTools and contributors.")),
        ("Poker-sang/WinUI3Utilities", LicenseTexts.MIT("2023", "Poker-sang")),
        ("dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.DependencyInjection", LicenseTexts.MIT("2024", ".NET Foundation and Contributors.")),
        ("codebude/QRCoder", LicenseTexts.MIT("2013-2018", "Raffael Herrmann")),
        ("microsoft/Microsoft.IO.RecyclableMemoryStream", LicenseTexts.MIT("2015-2016", "Microsoft")),
        ("microsoft/Win2D", LicenseTexts.MIT(string.Empty, "Microsoft Corporation. All rights reserved")),
        ("Sergio0694/PolySharp", LicenseTexts.MIT("2022", "Sergio Pedri")),
        ("SixLabors/ImageSharp", LicenseTexts.ImageSharp),
        ("xinntao/Real-ESRGAN", LicenseTexts.BSD3("2021", "Xintao Wang")),
        ("mbdavid/LiteDB", LicenseTexts.MIT("2014-2022", "Mauricio David")),
        ("davidxuang/FluentIcons", LicenseTexts.MIT("2022", "davidxuang")),
        ("LasmGratel/PininSharp", LicenseTexts.MIT("2021", "Lasm Gratel")),
        ("QuestPDF/QuestPDF", LicenseTexts.QuestPDF),
        ("dotnetcore/WebApiClient", LicenseTexts.MIT("2020", ".NET Core Community"))
    ];
    // ReSharper restore StringLiteralTypo

    public static IEnumerable<DependencyViewModel> DependencyViewModels =>
        _dependencies.Select(t =>
        {
            var segments = t.Item1.Split('/');
            return new DependencyViewModel(segments[^1], "by " + segments[0], "https://github.com/" + t.Item1, t.Item2);
        });
}
