#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/AppVersion.cs
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

namespace Pixeval.AppManagement;

public enum IterationStage
{
    Alpha,
    Preview,
    ReleaseCandidate,
    Stable
}

// A slightly modified semantic version
public record AppVersion(IterationStage Stage, int Major, int Minor, int Patch, int? PreReleaseSpecifier = null) : IComparable<AppVersion>
{
    public int CompareTo(AppVersion? other)
    {
        if (other is { } version)
        {
            if (version.Stage.CompareTo(Stage) is var i and not 0)
            {
                return i;
            }

            if (version.Major.CompareTo(Major) is var j and not 0)
            {
                return j;
            }

            if (version.Minor.CompareTo(Minor) is var k and not 0)
            {
                return k;
            }

            if (version.Patch.CompareTo(Patch) is var n and not 0)
            {
                return n;
            }

            if (version.PreReleaseSpecifier is { } thatSpecifier && PreReleaseSpecifier is { } specifier && thatSpecifier.CompareTo(specifier) is var m and not 0)
            {
                return m;
            }

            return 0;
        }

        return 1;
    }

    public override string ToString()
    {
        var versionNumber = $"v{Major}.{Minor}.{Patch}";
        var str = Stage switch
        {
            IterationStage.Alpha => $"{versionNumber}-alpha",
            IterationStage.Preview => $"{versionNumber}-preview",
            IterationStage.ReleaseCandidate => $"{versionNumber}-rc",
            IterationStage.Stable => $"{versionNumber}",
            _ => throw new ArgumentOutOfRangeException()
        };
        return Stage is not IterationStage.Stable && PreReleaseSpecifier is { } specifier ? $"{str}.{specifier}" : str;
    }
}