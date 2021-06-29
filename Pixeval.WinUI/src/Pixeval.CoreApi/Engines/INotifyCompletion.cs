using JetBrains.Annotations;

namespace Pixeval.CoreApi.Engines
{
    /// <summary>
    /// 代表一个可以指示是否已经完成的对象
    /// </summary>
    [PublicAPI]
    public interface INotifyCompletion
    { 
        bool IsCompleted { get; set; }
    }
}