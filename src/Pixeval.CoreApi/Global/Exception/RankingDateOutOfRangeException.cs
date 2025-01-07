// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

namespace Pixeval.CoreApi.Global.Exception;

/// <summary>
/// 搜索榜单时设定的日期大于等于当前日期-2天
/// </summary>
public class RankingDateOutOfRangeException : MakoException
{
    public RankingDateOutOfRangeException()
    {
    }

    public RankingDateOutOfRangeException(string? message) : base(message)
    {
    }

    public RankingDateOutOfRangeException(string? message, System.Exception? innerException) : base(message, innerException)
    {
    }
}
