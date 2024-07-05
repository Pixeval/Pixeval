#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/DownloadState.cs
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

namespace Pixeval.Download;

/// <summary>
/// 下载状态
/// </summary>
public enum DownloadState
{
    /// <summary>
    /// 正在等待下载
    /// </summary>
    Queued,

    /// <summary>
    /// 正在下载
    /// </summary>
    Running,

    /// <summary>
    /// 暂停下载
    /// </summary>
    Paused,

    /// <summary>
    /// 取消下载
    /// </summary>
    Cancelled,

    /// <summary>
    /// 下载错误
    /// </summary>
    Error,

    /// <summary>
    /// 完成下载，但还未进行后续处理
    /// </summary>
    Pending,

    /// <summary>
    /// 下载全部完成
    /// </summary>
    Completed
}
