// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Controls;
using Pixeval.Download.MacroParser;
using Pixeval.Util.IO;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IWorkViewModel>]
public class FileExtensionMacro : ITransducer<IWorkViewModel>, ILastSegment
{
    public const string NameConst = "ext";

    public const string NameConstToken = $".<{NameConst}>";

    public string Name => NameConst;

    public string Substitute(IWorkViewModel context)
    {
        return context switch
        {
            IllustrationItemViewModel illustrationItemViewModel => illustrationItemViewModel.IsUgoira
                ? IoHelper.GetUgoiraExtension()
                : IoHelper.GetIllustrationExtension(),
            NovelItemViewModel => IoHelper.GetNovelExtension(),
            _ => ""
        };
    }
}
