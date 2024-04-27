using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixeval.Filters
{
    internal class FilterSettingBuilder
    {
        public IEnumerable<QueryFilterToken> IncludeTags = new List<QueryFilterToken>();
        public IEnumerable<QueryFilterToken> ExcludeTags = new List<QueryFilterToken>();
        public long LeastBookmark = 0;
        public long MaximumBookmark = 0;
        public IEnumerable<QueryFilterToken> ExcludeUserName = new List<QueryFilterToken>();
        public QueryFilterToken IllustratorName = new("");
        public long IllustratorId = 0;
        public QueryFilterToken IllustrationName = new("");
        public long IllustrationId = 0;
        public DateTimeOffset PublishDateStart = DateTimeOffset.Now;
        public DateTimeOffset PublishDateEnd = DateTimeOffset.Now;

        public FilterSetting Build()
        {
            return new FilterSetting(
                IncludeTags, ExcludeTags, LeastBookmark, MaximumBookmark,
                ExcludeUserName, IllustratorName, IllustratorId,
                IllustrationName, IllustrationId,
                PublishDateStart, PublishDateEnd);
        }
    }
}
