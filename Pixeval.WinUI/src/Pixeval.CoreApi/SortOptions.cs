using JetBrains.Annotations;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi
{
    [PublicAPI]
    public enum IllustrationSortOption
    {
        [Description("popular_desc")]
        PopularityDescending,
        
        [Description("date_asc")]
        PublishDateAscending,
        
        [Description("date_desc")]
        PublishDateDescending
    }

    [PublicAPI]
    public enum UserSortOption
    {
        [Description("date_asc")]
        DateAscending,
        
        [Description("date_desc")]
        DateDescending
    }
}