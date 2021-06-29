using JetBrains.Annotations;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi
{
    [PublicAPI]
    public enum SearchDuration
    {
        [Description("within_last_day")]
        WithinLastDay,
        
        [Description("within_last_week")]
        WithinLastWeek,
        
        [Description("within_last_month")]
        WithinLastMonth,
    }
}