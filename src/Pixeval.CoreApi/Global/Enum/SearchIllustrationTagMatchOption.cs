// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.ComponentModel;

namespace Pixeval.CoreApi.Global.Enum;

public enum SearchIllustrationTagMatchOption
{
    [Description("partial_match_for_tags")]
    PartialMatchForTags,

    [Description("exact_match_for_tags")]
    ExactMatchForTags,

    [Description("title_and_caption")]
    TitleAndCaption
}
