using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pixeval.Controls;

namespace Pixeval.Filters
{
    internal record FilterSetting(
        IEnumerable<FilterToken> IncludeTags,
        IEnumerable<FilterToken> ExcludeTags,
        int LeastBookmark,
        int MaximumBookmark,
        IEnumerable<FilterToken> ExcludeUserName,
        FilterToken IllustratorName,
        long IllustratorId,
        FilterToken IllustrationName,
        long IllustrationId,
        DateTimeOffset PublishDateStart,
        DateTimeOffset PublishDateEnd
    ) { }
}
