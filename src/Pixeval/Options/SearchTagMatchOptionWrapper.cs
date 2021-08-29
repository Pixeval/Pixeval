using System.Collections.Generic;
using Mako.Global.Enum;
using Pixeval.Util;
using Pixeval.Util.Generic;

namespace Pixeval.Options
{
    public record SearchTagMatchOptionWrapper : ILocalizedBox<SearchTagMatchOption>
    {
        public SearchTagMatchOption Value { get; }

        public string LocalizedString { get; }

        public SearchTagMatchOptionWrapper(SearchTagMatchOption value, string localizedString)
        {
            Value = value;
            LocalizedString = localizedString;
        }

        public static IEnumerable<SearchTagMatchOptionWrapper> AvailableOptions()
        {
            return new SearchTagMatchOptionWrapper[]
            {
                new(SearchTagMatchOption.PartialMatchForTags, MiscResources.SearchTagMatchOptionPartialMatchForTags),
                new(SearchTagMatchOption.ExactMatchForTags, MiscResources.SearchTagMatchOptionExactMatchForTags),
                new(SearchTagMatchOption.TitleAndCaption, MiscResources.SearchTagMatchOptionTitleAndCaption)
            };
        }
    }
}