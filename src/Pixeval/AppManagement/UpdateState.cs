// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

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
