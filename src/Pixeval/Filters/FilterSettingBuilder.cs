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

        public override bool Equals(object? obj)
        {
            if (obj is FilterSettingBuilder fb)
            {
                return fb.IncludeTags.SequenceEqual(this.IncludeTags)
                       && fb.ExcludeTags.SequenceEqual(this.ExcludeTags)
                       && fb.LeastBookmark == this.LeastBookmark
                       && fb.MaximumBookmark == this.MaximumBookmark
                       && fb.ExcludeUserName.SequenceEqual(this.ExcludeUserName)
                       && fb.IllustratorName == this.IllustratorName
                       && fb.IllustrationName == this.IllustrationName
                       && fb.IllustratorId == this.IllustratorId
                       && fb.IllustrationId == this.IllustrationId
                       && fb.PublishDateStart == this.PublishDateStart
                       && fb.PublishDateEnd == this.PublishDateEnd;
            }

            return false;
        }

    }
}
