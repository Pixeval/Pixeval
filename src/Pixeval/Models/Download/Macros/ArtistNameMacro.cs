// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Linq;
using Misaki;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.I18N;

namespace Pixeval.Models.Download.Macros;

[MetaPathMacro]
public class ArtistNameMacro : ITransducer<IArtworkInfo>
{
    public string Name => "artist_name";

    public string Description => I18NManager.GetResource(MacroParserResources.MacroDescriptionArtistName);

    public bool IsFormatterValid(string? formatter) => MacroHelper.IsStringFormatterValid(formatter);

    public string Substitute(IArtworkInfo context, string? formatter, out bool includeToken)
    {
        includeToken = false;
        return MacroHelper.FormatString(
            string.Join(',', context.Authors
                .Select(t => t.Name)), formatter);
    }
}
