using JetBrains.Annotations;

namespace Pixeval.CoreApi.Engines
{
    /// <summary>
    /// 提供一个在任务结束时被执行的回调，可以附带一个参数
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    [PublicAPI]
    public interface ICompletionCallback<in T>
    {
        void OnCompletion(T param);
    }
}