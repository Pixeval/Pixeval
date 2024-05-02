using System.Collections.Generic;
using System.Linq;

namespace Pixeval.Pages.Misc;

public record DependencyViewModel(string Name, string Author, string Url)
{
    // ReSharper disable StringLiteralTypo
    private static readonly string[] _dependencies =
    [
        "CommunityToolkit",
        "praeclarum/sqlite-net",
        "mysticmind/reversemarkdown-net",
        "GitTools/GitVersion",
        "Poker-sang/WinUI3Utilities",
        "dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.DependencyInjection",
        "codebude/QRCoder",
        "microsoft/Microsoft.IO.RecyclableMemoryStream",
        "microsoft/Win2D",
        "Sergio0694/PolySharp",
        "SixLabors/ImageSharp",
        "reactiveui/refit"
    ];
    // ReSharper restore StringLiteralTypo

    public static IEnumerable<DependencyViewModel> DependencyViewModels =>
        _dependencies.Select(t =>
        {
            var segments = t.Split('/');
            return new DependencyViewModel(segments[^1], "by " + segments[0], "https://github.com/" + t);
        });
}
