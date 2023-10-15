using BBFramework.Pool;
using com.bbbirder;
using UnityEngine;

namespace BBFramework.Pool
{
    /// <summary>
    /// 池项，AsyncPoolBase的伴生接口。
    /// </summary>
    /// <remarks>
    /// <para>实现此接口注意：</para>
    /// <para>1. 实例可用时不要忘记调用`NotifyInternalCreated`</para>
    /// <para>实现此接口后，四个回调将满足以下约定：</para>
    /// <para>1. `OnCreate()`总是第一个触发（`NotifyInternalCreated`调用后）,且只有一次</para>
    /// <para>2. `OnSpawn()`和`OnRecycle()`总是按序成对触发，可能重复多次</para>
    /// <para>3. `OnDestroy()`总是最后一个触发，且只有一次</para>
    /// <para>即：OnCreate, [OnSpawn,OnRecycle] * N, OnDestroy</para>
    /// </remarks>
    public interface IPooledItem : IDirectRetrieve
    {
        internal bool IsInternalCreated { get; set; }
        internal IPool pool { get; set; }

        internal void NotifyInternalCreated()
        {
            IsInternalCreated = true;
            // pool.NotifyInternalCreated(this);
        }

        /// <summary>
        /// 实现真正的销毁逻辑
        /// </summary>
        internal void Destroy();

        /// <summary>
        /// 实例被创建并可用
        /// </summary>
        void OnCreate();
        /// <summary>
        /// 实例从池中产生
        /// </summary>
        void OnSpawn();
        /// <summary>
        /// 实例回收到池中
        /// </summary>
        void OnRecycle();
        /// <summary>
        /// 实例被销毁
        /// </summary>
        void OnDestroy();

    }

    public interface IPooledItem<T> : IPooledItem { }
}
