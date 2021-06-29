using System.Collections.Generic;
using JetBrains.Annotations;

namespace Pixeval.CoreApi.Engines
{
    [PublicAPI]
    public interface IListSupport<E>
    {
        /// <summary>
        /// 把一个<typeparamref name="E"/>插入到<see cref="IList{E}"/>中
        /// 可以在这个方法里选择把<typeparamref name="E"/>插入到<see cref="IList{E}"/>
        /// 中的合适位置
        /// </summary>
        /// <param name="list">被插入的列表</param>
        /// <param name="item">要插入的元素</param>
        void InsertTo(IList<E> list, E? item);
    }
}