#region Copyright (c) Pixeval/Pixeval.Controls
// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2024 Pixeval.Controls/TimelineAxisPlacement.cs
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

namespace Pixeval.Controls.Timeline;

public enum TimelineAxisPlacement : ObservableObject
{
    Left, Right
}

public static class TimelineAxisPlacementExtension
{
    public static TimelineAxisPlacement Inverse(this TimelineAxisPlacement placement)
    {
        return placement switch
        {
            TimelineAxisPlacement.Left => TimelineAxisPlacement.Right,
            TimelineAxisPlacement.Right => TimelineAxisPlacement.Left,
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };
    }
}
