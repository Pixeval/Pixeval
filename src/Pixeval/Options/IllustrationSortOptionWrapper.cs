using System.Collections.Generic;
using System.Linq;
using Mako.Global.Enum;
using Pixeval.Util;
using Pixeval.Util.Generic;

namespace Pixeval.Options
{
    public record IllustrationSortOptionWrapper : ILocalizedBox<IllustrationSortOption, IllustrationSortOptionWrapper>
    {
        public IllustrationSortOption Value { get; }

        public string LocalizedString { get; }

        public static IEnumerable<IllustrationSortOptionWrapper> AvailableOptions()
        {
            return new IllustrationSortOptionWrapper[]
            {
                new(IllustrationSortOption.PublishDateDescending, MiscResources.IllustrationSortOptionPublishDateDescending),
                new(IllustrationSortOption.PublishDateAscending, MiscResources.IllustrationSortOptionPublishDateAscending),
                new(IllustrationSortOption.PopularityDescending, MiscResources.IllustrationSortOptionPopularityDescending),
                new(IllustrationSortOption.DoNotSort, MiscResources.IllustrationSortOptionDoNotSort)
            };
        }

        public IllustrationSortOptionWrapper(IllustrationSortOption value, string localizedString)
        {
            Value = value;
            LocalizedString = localizedString;
        }
    }
}