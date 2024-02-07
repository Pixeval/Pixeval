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
using System.Diagnostics.CodeAnalysis;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.AppManagement;

public enum IterationStage
{
    Preview,
    Stable
}

/// <summary>
/// A slightly modified semantic version
/// </summary>
/// <param name="major"></param>
/// <param name="minor"></param>
/// <param name="patch"></param>
/// <param name="stage"></param>
/// <param name="preReleaseSpecifier"></param>
public class AppVersion(int major, int minor, int patch, IterationStage stage, int? preReleaseSpecifier = null) : IComparable<AppVersion>, IEquatable<AppVersion>, IParsable<AppVersion>
{
    public int Major { get; } = major;

    public int Minor { get; } = minor;

    public int Patch { get; } = patch;

    public IterationStage Stage => PreReleaseSpecifier is null ? IterationStage.Stable : IterationStage.Preview;

    public int? PreReleaseSpecifier { get; } = preReleaseSpecifier;

    public int CompareTo(AppVersion? other)
    {
        if (other is null)
            return 1;

        if (Major.CompareTo(other.Major) is var j and not 0)
            return j;

        if (Minor.CompareTo(other.Minor) is var k and not 0)
            return k;

        if (Patch.CompareTo(other.Patch) is var n and not 0)
            return n;

        if (other.PreReleaseSpecifier is { } thatSpecifier && PreReleaseSpecifier is { } specifier && specifier.CompareTo(thatSpecifier) is var m and not 0)
            return m;

        return 0;
    }

    public UpdateState CompareUpdateState(AppVersion? newVersion)
    {
        if (newVersion is null)
            return UpdateState.Unknown;

        return CompareTo(newVersion) switch
        {
            > 0 => UpdateState.Insider,
            0 => UpdateState.UpToDate,
            _ => newVersion.Major > Major ? UpdateState.MajorUpdate :
                newVersion.Minor > Minor ? UpdateState.MinorUpdate :
                newVersion.Patch > Patch ? UpdateState.PatchUpdate :
                UpdateState.SpecifierUpdate
        };
    }

    public static AppVersion Parse(string s) => Parse(s, null);

    public static bool TryParse([NotNullWhen(true)] string? s, [NotNullWhen(true)] out AppVersion? result) => TryParse(s, null, out result);

    public static AppVersion Parse(string s, IFormatProvider? provider)
    {
        return TryParse(s, provider, out var result) ? result : ThrowUtils.Format<AppVersion>();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [NotNullWhen(true)] out AppVersion? result)
    {
        result = null;
        if (s?.Split('-') is not [var version, .. var specifierStr])
            return false;


        if (version.Split('.') is not [var majorStr, var minorStr, var patchStr] ||
            !int.TryParse(majorStr, out var major) ||
            !int.TryParse(minorStr, out var minor) ||
            !int.TryParse(patchStr, out var patch))
        {
            return false;
        }

        switch (specifierStr)
        {
            case [var specifier]:
            {
                if (specifier.Split('.') is not [var stage, .. var rest] ||
                    !Enum.TryParse<IterationStage>(stage, true, out var iterationStage))
                    return false;
                switch (rest)
                {
                    case []:
                    {
                        result = new AppVersion(major, minor, patch, iterationStage);
                        return true;
                    }
                    case [var preReleaseSpecifierStr]:
                    {
                        if (!int.TryParse(preReleaseSpecifierStr, out var preReleaseSpecifier))
                            return false;

                        result = new AppVersion(major, minor, patch, iterationStage, preReleaseSpecifier);
                        return true;
                    }
                    default:
                        return false;
                }
            }
            case []:
                result = new AppVersion(major, minor, patch, IterationStage.Stable);
                return true;
            default:
                return false;
        }
    }

    public static bool operator >(AppVersion x, AppVersion y) => x.CompareTo(y) > 0;

    public static bool operator <(AppVersion x, AppVersion y) => x.CompareTo(y) < 0;

    public static bool operator ==(AppVersion x, AppVersion y) => x.Equals(y);

    public static bool operator !=(AppVersion x, AppVersion y) => !x.Equals(y);

    public override string ToString()
    {
        var versionNumber = $"{Major}.{Minor}.{Patch}";
        if (PreReleaseSpecifier is { } specifier)
            versionNumber += "-preview" + specifier;
        return versionNumber;
    }

    public bool Equals(AppVersion? other) => other is not null && Major == other.Major && Minor == other.Minor && Patch == other.Patch && PreReleaseSpecifier == other.PreReleaseSpecifier;

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        return Equals(obj as AppVersion);
    }

    public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch, PreReleaseSpecifier);
}

public enum UpdateState
{
    /// <summary>
    /// 已是最新
    /// </summary>
    UpToDate,

    /// <summary>
    /// 主要更新
    /// </summary>
    MajorUpdate,

    /// <summary>
    /// 次要更新
    /// </summary>
    MinorUpdate,

    /// <summary>
    /// 修订更新
    /// </summary>
    PatchUpdate,

    /// <summary>
    /// 预览更新
    /// </summary>
    SpecifierUpdate,

    /// <summary>
    /// 内部版本
    /// </summary>
    Insider,

    /// <summary>
    /// 未知状态
    /// </summary>
    Unknown
}
