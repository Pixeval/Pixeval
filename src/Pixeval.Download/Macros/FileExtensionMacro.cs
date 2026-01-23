// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IArtworkInfo>]
public class FileExtensionMacro : ITransducer<IArtworkInfo>, ILastSegment
{
    public const string NameConst = "ext";

    public const string NameConstToken = $".<{NameConst}>";

    public string Name => NameConst;

    public string Substitute(IArtworkInfo context) => NameConstToken;
}
