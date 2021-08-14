using System.Collections.Generic;
using Mako.Global.Enum;
using Pixeval.Util;

namespace Pixeval.Options
{
    public record IllustrationSortOptionWrapper : ILocalizedBox<IllustrationSortOption>
    {
        /// <summary>
        /// The <see cref="IllustrationSortOption"/> is welded into Mako, we cannot add <see cref="LocalizedResource"/> on its fields
        /// so an alternative is chosen
        /// And we cannot use generic since the xaml compiler will refuse to compile
        /// </summary>
        public static readonly IEnumerable<IllustrationSortOptionWrapper> Available = new IllustrationSortOptionWrapper[]
        {
            new(IllustrationSortOption.PublishDateDescending, MiscResources.IllustrationSortOptionPublishDateDescending),
            new(IllustrationSortOption.PublishDateAscending, MiscResources.IllustrationSortOptionPublishDateAscending),
            new(IllustrationSortOption.PopularityDescending, MiscResources.IllustrationSortOptionPopularityDescending),
            new(IllustrationSortOption.DoNotSort, MiscResources.IllustrationSortOptionDoNotSort)
        };

        public IllustrationSortOption Value { get; }

        public string LocalizedString { get; }

        public IllustrationSortOptionWrapper(IllustrationSortOption value, string localizedString)
        {
            Value = value;
            LocalizedString = localizedString;
        }
    }
}