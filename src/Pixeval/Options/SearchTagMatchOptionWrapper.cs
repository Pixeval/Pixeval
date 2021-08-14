using System.Collections.Generic;
using Mako.Global.Enum;
using Pixeval.Util;

namespace Pixeval.Options
{
    public record SearchTagMatchOptionWrapper : ILocalizedBox<SearchTagMatchOption>
    {
        public static readonly IEnumerable<SearchTagMatchOptionWrapper> Available = new SearchTagMatchOptionWrapper[]
        {
            new(SearchTagMatchOption.PartialMatchForTags, MiscResources.SearchTagMatchOptionPartialMatchForTags),
            new(SearchTagMatchOption.ExactMatchForTags, MiscResources.SearchTagMatchOptionExactMatchForTags),
            new(SearchTagMatchOption.TitleAndCaption, MiscResources.SearchTagMatchOptionTitleAndCaption)
        };

        public SearchTagMatchOption Value { get; }

        public string LocalizedString { get; }

        public SearchTagMatchOptionWrapper(SearchTagMatchOption value, string localizedString)
        {
            Value = value;
            LocalizedString = localizedString;
        }
    }
}