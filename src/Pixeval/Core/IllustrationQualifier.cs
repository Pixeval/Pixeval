#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Linq;
using Pixeval.Data.ViewModel;

namespace Pixeval.Core
{
    public class IllustrationQualifier : IQualifier<Illustration, IllustrationQualification>
    {
        public bool Qualified(Illustration condition, IllustrationQualification pattern)
        {
            return pattern switch
            {
                { Type: ConditionType.Id } => !condition.Id.Contains(pattern.Condition),
                { Type: ConditionType.Tag } => !condition.Title.Contains(pattern.Condition) &&
                !(condition.Tags != null &&
                    condition.Tags.Any(tag =>
                                           tag?.Name != null && tag.Name.Contains(pattern.Condition) ||
                                           tag?.TranslatedName != null &&
                                           tag.TranslatedName.Contains(pattern.Condition))),
                { Type: ConditionType.ExcludeTag } => condition.Tags != null &&
                condition.Tags.Any(tag =>
                                       tag?.Name != null &&
                                       tag.Name.Contains(pattern.Condition[1..]) ||
                                       tag?.TranslatedName != null &&
                                       tag.TranslatedName.Contains(pattern.Condition[1..])),
                _ => false
            };
        }
    }
}