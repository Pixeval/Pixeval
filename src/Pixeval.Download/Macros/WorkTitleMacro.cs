// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IArtworkInfo>]
public class WorkTitleMacro : ITransducer<IArtworkInfo>
{
    public string Name => "title";

    public string Substitute(IArtworkInfo context) => MacroHelper.NormalizePathSegmentInMacro(context.Title);
}
