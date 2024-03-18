#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/SortOptions.cs
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

using System.ComponentModel;

namespace Pixeval.CoreApi.Global.Enum;

public enum WorkSortOption
{
    DoNotSort,

    [Description("popular_desc")]
    PopularityDescending,

    [Description("date_asc")]
    PublishDateAscending,

    [Description("date_desc")]
    PublishDateDescending
}

public enum UserSortOption
{
    DoNotSort,

    [Description("date_asc")]
    DateAscending,

    [Description("date_desc")]
    DateDescending
}
