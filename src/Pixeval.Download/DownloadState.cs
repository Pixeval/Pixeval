// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

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
