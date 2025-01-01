#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2024 Pixeval.CoreApi/BookmarkTag.cs
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
using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Model;

[Factory]
public partial record BookmarkTag : IEntry, IEquatable<string>
{
    [JsonPropertyName("name")]
    public required string Name { get; set; } = "";

    [JsonPropertyName("count")]
    public required int Count { get; set; }

    public static string AllCountedTagString { get; set; } = null!;

    public override string ToString() => $"{Name} ({Count})";

    public virtual bool Equals(BookmarkTag? other)
    {
        return other is not null && (ReferenceEquals(this, other) || Name == other.Name);
    }

    public virtual bool Equals(string? other)
    {
        return other is not null && Name == other;
    }

    /// <summary>
    /// ReSharper disable once NonReadonlyMemberInGetHashCode
    /// </summary>
    public override int GetHashCode() => Name.GetHashCode();
}
