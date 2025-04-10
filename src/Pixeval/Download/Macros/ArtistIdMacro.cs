// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using Misaki;
using Pixeval.Download.MacroParser;
using Pixeval.Util.IO;
using Pixeval.Utilities;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IArtworkInfo>]
public class ArtistIdMacro : ITransducer<IArtworkInfo>
{
    public string Name => "artist_id";

    public string Substitute(IArtworkInfo context) =>
        ((IEnumerable<IUser>) context.Authors)
        .Select(t => t.Name)
        .Aggregate((x, y) => x + ',' + y)
        .Let(IoHelper.NormalizePathSegment);
}
