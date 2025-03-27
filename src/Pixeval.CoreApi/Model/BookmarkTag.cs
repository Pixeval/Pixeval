// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System;
using System.Text.Json.Serialization;

namespace Mako.Model;

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
