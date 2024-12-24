#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/UpdateState.cs
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

namespace Pixeval.AppManagement;

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
    /// 生成更新
    /// </summary>
    BuildUpdate,

    /// <summary>
    /// 修订更新
    /// </summary>
    RevisionUpdate,

    /// <summary>
    /// 内部版本
    /// </summary>
    Insider,

    /// <summary>
    /// 未知状态
    /// </summary>
    Unknown
}
