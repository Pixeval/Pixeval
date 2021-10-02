using System;
using System.Collections.Generic;
using System.Linq;
using Pixeval.CoreApi.Global.Enum;

namespace Pixeval.Controls.Setting.UI.Model
{
    public record SearchTagMatchOptionSettingEntryItem : IStringRepresentableItem
    {
        public static readonly IEnumerable<IStringRepresentableItem> AvailableItems = Enum.GetValues<SearchTagMatchOption>().Select(i => new SearchTagMatchOptionSettingEntryItem(i));

        public SearchTagMatchOptionSettingEntryItem(SearchTagMatchOption item)
        {
            Item = item;
            StringRepresentation = item switch
            {
                SearchTagMatchOption.PartialMatchForTags => MiscResources.SearchTagMatchOptionPartialMatchForTags,
                SearchTagMatchOption.ExactMatchForTags => MiscResources.SearchTagMatchOptionExactMatchForTags,
                SearchTagMatchOption.TitleAndCaption => MiscResources.SearchTagMatchOptionTitleAndCaption,
                _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
            };
        }

        public object Item { get; }

        public string StringRepresentation { get; }
    }
}