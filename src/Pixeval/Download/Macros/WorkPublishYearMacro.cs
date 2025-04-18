// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IArtworkInfo>]
public class WorkPublishYearMacro : ITransducer<IArtworkInfo>
{
    public string Name => "publish_year";

    public string Substitute(IArtworkInfo context) => context.CreateDate.Year.ToString();
}
