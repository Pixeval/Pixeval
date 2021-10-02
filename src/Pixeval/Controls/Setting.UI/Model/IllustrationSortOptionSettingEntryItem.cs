using System;
using System.Collections.Generic;
using System.Linq;
using Pixeval.CoreApi.Global.Enum;

namespace Pixeval.Controls.Setting.UI.Model
{
    public record IllustrationSortOptionSettingEntryItem : IStringRepresentableItem
    {
        public static readonly IEnumerable<IStringRepresentableItem> AvailableItems = Enum.GetValues<IllustrationSortOption>().Select(i => new IllustrationSortOptionSettingEntryItem(i));

        public IllustrationSortOptionSettingEntryItem(IllustrationSortOption item)
        {
            Item = item;
            StringRepresentation = item switch
            {
                IllustrationSortOption.PopularityDescending => MiscResources.IllustrationSortOptionPopularityDescending,
                IllustrationSortOption.PublishDateAscending => MiscResources.IllustrationSortOptionPublishDateAscending,
                IllustrationSortOption.PublishDateDescending => MiscResources.IllustrationSortOptionPublishDateDescending,
                IllustrationSortOption.DoNotSort => MiscResources.IllustrationSortOptionDoNotSort,
                _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
            };
        }

        public object Item { get; }

        public string StringRepresentation { get; }
    }
}