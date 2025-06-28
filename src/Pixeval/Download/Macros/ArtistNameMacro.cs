// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Linq;
using Misaki;
using Pixeval.Download.MacroParser;
using Pixeval.Util.IO;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IArtworkInfo>]
public class ArtistNameMacro : ITransducer<IArtworkInfo>
{
    public string Name => "artist_name";

    public string Substitute(IArtworkInfo context) =>
        IoHelper.NormalizePathSegmentInMacro(
            string.Join(',', context.Authors
                .Select(t => t.Name)));
}
