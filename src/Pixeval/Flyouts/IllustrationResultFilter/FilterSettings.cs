#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/FilterSettings.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using Pixeval.Controls.TokenInput;
using Pixeval.Utilities;

namespace Pixeval.Flyouts.IllustrationResultFilter;

/// <summary>
/// 
/// </summary>
/// <param name="IncludeTags"></param>
/// <param name="ExcludeTags"></param>
/// <param name="LeastBookmark"></param>
/// <param name="MaximumBookmark"></param>
/// <param name="UserGroupName"></param>
/// <param name="IllustratorName"></param>
/// <param name="IllustratorId">-1表未设置</param>
/// <param name="IllustrationName"></param>
/// <param name="IllustrationId">-1表未设置</param>
/// <param name="PublishDateStart"></param>
/// <param name="PublishDateEnd"></param>
public record FilterSettings(
    IEnumerable<Token> IncludeTags,
    IEnumerable<Token> ExcludeTags,
    int LeastBookmark,
    int MaximumBookmark,
    IEnumerable<Token> UserGroupName,
    Token IllustratorName,
    long IllustratorId,
    Token IllustrationName,
    long IllustrationId,
    DateTimeOffset PublishDateStart,
    DateTimeOffset PublishDateEnd)
{
    public static readonly FilterSettings Default = new(
        [],
        [],
        0,
        int.MaxValue,
        [],
        Token.Empty,
        -1,
        Token.Empty,
        -1,
        DateTimeOffset.MinValue,
        DateTimeOffset.MaxValue);

    public virtual bool Equals(FilterSettings? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return IncludeTags.SequenceEquals(other.IncludeTags, SequenceComparison.Unordered) &&
               ExcludeTags.SequenceEquals(other.ExcludeTags, SequenceComparison.Unordered) &&
               LeastBookmark == other.LeastBookmark &&
               MaximumBookmark == other.MaximumBookmark &&
               UserGroupName.SequenceEquals(other.UserGroupName, SequenceComparison.Unordered) &&
               IllustratorName.Equals(other.IllustratorName) &&
               IllustratorId == other.IllustratorId &&
               IllustrationName.Equals(other.IllustrationName) &&
               IllustrationId == other.IllustrationId &&
               PublishDateStart.Equals(other.PublishDateStart) &&
               PublishDateEnd.Equals(other.PublishDateEnd);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(IncludeTags);
        hashCode.Add(ExcludeTags);
        hashCode.Add(LeastBookmark);
        hashCode.Add(MaximumBookmark);
        hashCode.Add(UserGroupName);
        hashCode.Add(IllustratorName);
        hashCode.Add(IllustratorId);
        hashCode.Add(IllustrationName);
        hashCode.Add(IllustrationId);
        hashCode.Add(PublishDateStart);
        hashCode.Add(PublishDateEnd);
        return hashCode.ToHashCode();
    }
}
