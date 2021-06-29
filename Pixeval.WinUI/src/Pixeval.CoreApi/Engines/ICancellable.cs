using System.Threading;
using JetBrains.Annotations;

namespace Pixeval.CoreApi.Engines
{
    /// <summary>
    /// 表示一个可以被取消的任务
    /// </summary>
    [PublicAPI]
    public interface ICancellable
    {
        CancellationTokenSource CancellationTokenSource { get; set; }
    }
}