using JetBrains.Annotations;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi
{
    [PublicAPI]
    public enum SearchTagMatchOption
    {
        [Description("partial_match_for_tags")]
        PartialMatchForTags,
        
        [Description("exact_match_for_tags")]
        ExactMatchForTags,
        
        [Description("title_and_caption")]
        TitleAndCaption
    }
}