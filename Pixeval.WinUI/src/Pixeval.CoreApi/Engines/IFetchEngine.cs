using System.Collections.Generic;
using JetBrains.Annotations;

namespace Pixeval.CoreApi.Engines
{
    /// <summary>
    /// 一个抽象的，针对相同类型对象的搜索引擎，本质上是一个<see cref="IAsyncEnumerable{E}"/>
    /// <para>
    /// 为了保证UI的流畅度，该API使用边搜索边插入的策略，每搜索到一个新的元素就立即反馈给UI，而不是等到所有元素均搜索完毕后一并返回
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// IFetchEngine&lt;E&gt; engine = GetFetchEngine();
    /// await foreach (var element in engine)
    /// {
    ///     ProcessElement(element);
    /// }
    /// </code>
    /// </example>
    /// <typeparam name="E">搜索引擎所搜索的对象类型</typeparam>
    [PublicAPI]
    public interface IFetchEngine<out E> : IAsyncEnumerable<E>, IMakoClientSupport, IEngineHandleSource
    {
        /// <summary>
        /// 指示该引擎已经搜索了多少页，每页都会包含多个<see cref="E"/>实例
        /// </summary>
        int RequestedPages { get; set; }
    }
}