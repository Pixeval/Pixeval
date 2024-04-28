using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixeval.Filters;

internal class FilterSettingBuilder
{
    public IEnumerable<QueryFilterToken> IncludeTags = [];
    public IEnumerable<QueryFilterToken> ExcludeTags = [];
    public long LeastBookmark = 0;
    public long MaximumBookmark = 0;
    public IEnumerable<QueryFilterToken> ExcludeUserName = [];
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
        return obj is FilterSettingBuilder fb
               && fb.IncludeTags.SequenceEqual(IncludeTags)
               && fb.ExcludeTags.SequenceEqual(ExcludeTags)
               && fb.LeastBookmark == LeastBookmark
               && fb.MaximumBookmark == MaximumBookmark
               && fb.ExcludeUserName.SequenceEqual(ExcludeUserName)
               && fb.IllustratorName == IllustratorName
               && fb.IllustrationName == IllustrationName
               && fb.IllustratorId == IllustratorId
               && fb.IllustrationId == IllustrationId
               && fb.PublishDateStart == PublishDateStart
               && fb.PublishDateEnd == PublishDateEnd;
    }
}
