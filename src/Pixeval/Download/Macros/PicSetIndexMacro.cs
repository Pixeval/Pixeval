// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Controls;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IWorkViewModel>]
public class PicSetIndexMacro : ITransducer<IWorkViewModel>, ILastSegment
{
    public const string NameConst = "pic_set_index";

    public const string NameConstToken = $"<{NameConst}>";

    public string Name => NameConst;

    public string Substitute(IWorkViewModel context)
    {
        // 下载单张漫画的时候，MangaIndex 不为 -1
        // 下载多张漫画或者单张插画的时候，为 -1
        return context is IllustrationItemViewModel { IsManga: true, MangaIndex: not -1 } i ? i.MangaIndex.ToString() : $"<{Name}>";
    }
}
