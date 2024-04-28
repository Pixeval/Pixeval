using System;
using System.Collections.Generic;

namespace Pixeval.Filters;

internal record FilterSetting(
    IEnumerable<QueryFilterToken> IncludeTags,
    IEnumerable<QueryFilterToken> ExcludeTags,
    long LeastBookmark,
    long MaximumBookmark,
    IEnumerable<QueryFilterToken> ExcludeUserName,
    QueryFilterToken IllustratorName,
    long IllustratorId,
    QueryFilterToken IllustrationName,
    long IllustrationId,
    DateTimeOffset PublishDateStart,
    DateTimeOffset PublishDateEnd
);
