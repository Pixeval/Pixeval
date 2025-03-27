// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.ComponentModel;

namespace Mako.Global.Enum;

public enum SearchIllustrationTagMatchOption
{
    [Description("partial_match_for_tags")]
    PartialMatchForTags,

    [Description("exact_match_for_tags")]
    ExactMatchForTags,

    [Description("title_and_caption")]
    TitleAndCaption
}
