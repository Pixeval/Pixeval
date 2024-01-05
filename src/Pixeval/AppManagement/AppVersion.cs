#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/AppVersion.cs
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
using WinUI3Utilities;

namespace Pixeval.AppManagement;

public enum IterationStage
{
    Preview,
    Stable
}

// A slightly modified semantic version
internal record AppVersion(int Major, int Minor, int Patch, IterationStage Stage, int? PreReleaseSpecifier = null) : IComparable<AppVersion>
{
    public int CompareTo(AppVersion? other)
    {
        if (other != null)
        {
            if (other.Stage.CompareTo(Stage) is var i and not 0)
            {
                return i;
            }

            if (other.Major.CompareTo(Major) is var j and not 0)
            {
                return j;
            }

            if (other.Minor.CompareTo(Minor) is var k and not 0)
            {
                return k;
            }

            if (other.Patch.CompareTo(Patch) is var n and not 0)
            {
                return n;
            }

            if (other.PreReleaseSpecifier is { } thatSpecifier && PreReleaseSpecifier is { } specifier && thatSpecifier.CompareTo(specifier) is var m and not 0)
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
            IterationStage.Preview => $"{versionNumber}-preview",
            IterationStage.Stable => $"{versionNumber}",
            _ => ThrowHelper.ArgumentOutOfRange<IterationStage, string>(Stage)
        };
        return Stage is not IterationStage.Stable && PreReleaseSpecifier is { } specifier ? $"{str}.{specifier}" : str;
    }
}
