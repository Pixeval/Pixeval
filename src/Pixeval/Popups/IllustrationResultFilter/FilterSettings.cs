using System;
using System.Collections.Generic;
using Pixeval.UserControls.TokenInput;

namespace Pixeval.Popups.IllustrationResultFilter
{
    public record FilterSettings(
        IEnumerable<Token> IncludeTags,
        IEnumerable<Token> ExcludeTags,
        int LeastBookmark,
        int MaximumBookmark,
        IEnumerable<Token> UserGroupName,
        Token IllustratorName,
        string IllustratorId,
        Token IllustrationName,
        string IllustrationId,
        DateTimeOffset PublishDateStart,
        DateTimeOffset PublishDateEnd);
}