using System;
using System.Collections;
using System.Collections.Generic;
using com.bbbirder;

namespace BBFramework.Pool
{
    public enum ClearReason
    {
        /// <summary>
        /// 手动强制清除
        /// </summary>
        Manual,
        /// <summary>
        /// 系统内存警告
        /// </summary>
        LowMemory,
    }
    
    public interface ICache : IEnumerable<(Type type, int pooled, int outside)>, IDirectRetrieve
    {
        internal IItemFactory itemFactory {get;set;}

        bool Contains(object item);
        void Clear(ClearReason reason = ClearReason.Manual);

        /// <summary>
        /// 从缓存中移除并返回一个 <typeparamref name="T"/> 元素, 不存在则创建
        /// </summary>
        /// <typeparam name="T">元素类型，如果只使用一种类型，可忽略</typeparam>
        T Rent<T>();

        /// <summary>
        /// 添加对象到缓存中
        /// </summary>
        /// <param name="inst"></param>
        void Return(object item);


        new IEnumerator<(Type type, int pooled, int outside)> GetEnumerator()
        {
            yield break;
        }
        
        IEnumerator<(Type type, int pooled, int outside)> IEnumerable<(Type type, int pooled, int outside)>.GetEnumerator()
            => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

    }
}
