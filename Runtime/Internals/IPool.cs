using System;
using System.Collections.Generic;
using com.bbbirder;
using com.bbbirder.injection;

//TODO: test cache di

namespace BBFramework.Pool
{

    /// <summary>
    /// 真正执行创建和销毁的类
    /// </summary>
    public interface IItemFactory
    {
        internal void DestroyItem(object item);
        T CreateItem<T>();
    }

    public interface IPool : IItemFactory, IDirectRetrieve
    {
        ICache Cache { get; }
        IEnumerator<(Type type, int pooledCount, int outsideCount)> GetEnumerator() => Cache?.GetEnumerator();
        void Clear(ClearReason reason = ClearReason.Manual){
            Cache.Clear(reason);
        }

    }

    public interface IPool<T> : IPool
    {
        TSpec Get<TSpec>() where TSpec : T;
        void Recycle(T item);
    }
}
