// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IArtworkInfo>]
public class WorkIdMacro : ITransducer<IArtworkInfo>
{
    public string Name => "id";

    public string Substitute(IArtworkInfo context) => context.Id;
}
