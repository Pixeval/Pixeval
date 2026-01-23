// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.Models.Database;

public class SearchHistoryEntry : HistoryEntry
{
    /// <summary>
    /// Search value
    /// </summary>
    public string Value { get; init; } = "";

    public string? TranslatedName { get; init; }

    public DateTime Time { get; init; }
}
