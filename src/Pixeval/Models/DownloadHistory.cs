#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/DownloadHistory.cs
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
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pixeval.Models;

public record DownloadHistory
{
    public Guid? Id { get; init; }

    public string? Destination { get; init; }

    public DownloadItemType Type { get; init; }

    public string? IllustrationId { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    public string? Url { get; init; }

    public string? Thumbnail { get; init; }
}

public enum DownloadItemType
{
    Manga,
    Ugoira,
    Illustration
}