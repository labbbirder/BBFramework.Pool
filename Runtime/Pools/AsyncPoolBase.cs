using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using com.bbbirder.injection;
using UnityEngine;

namespace BBFramework.Pool
{
    /// <summary>
    /// 实现了最基础功能，并保证了接口回调顺序的抽象池类
    /// </summary>
    [Serializable]
    public abstract class AsyncPoolBase<T> : IPool<T> where T : IPooledItem, new()
    {
        ICache IPool.Cache => Cache;
        protected virtual ICache Cache { get; set; }

        public TSpec Get<TSpec>() where TSpec : T
        {
            var inst = Cache.Rent<TSpec>();
            inst.pool = this;

            if (inst.IsInternalCreated)
            {
                inst.OnSpawn();
            }
            return inst;
        }


        public void Recycle(T item)
        {
            if (Cache.Contains(item))
            {
                throw new Exception($"reject to recycle {item}, item is already recycled. Pool: {this}");
            }
            Cache.Return(item);
            if (item.IsInternalCreated)
            {
                item.OnRecycle();
            }
        }


        public void NotifyInternalCreated(IPooledItem item)
        {
            item.OnCreate();
            if (!Cache.Contains(item))
            {
                item.OnSpawn();
            }
        }
        protected abstract void DestroyItem(T item);

        protected abstract TSpec CreateItem<TSpec>() where TSpec : T;

        Dictionary<Type, Delegate> creators = new();
        MethodInfo CreateItemMethod;
        TSpec IItemFactory.CreateItem<TSpec>()
        {
            var targetType = typeof(TSpec);
            if (!typeof(T).IsAssignableFrom(targetType))
            {
                throw new ArgumentException($"cannot create {targetType} in Pool {this}");
            }
            CreateItemMethod ??= this.GetType().GetMethod(nameof(CreateItem), BindingFlags.Instance | BindingFlags.NonPublic);
            if (!creators.TryGetValue(targetType, out var func))
            {
                Debug.Log(CreateItemMethod);
                Debug.Log(CreateItemMethod.MakeGenericMethod(targetType));
                creators[targetType] = func = CreateItemMethod.MakeGenericMethod(targetType).CreateDelegate(typeof(Func<TSpec>), this);
            }
            return (func as Func<TSpec>).Invoke();
        }
        void IItemFactory.DestroyItem(object item)
        {
            var pooledItem = (T)item;
            if (pooledItem.IsInternalCreated)
            {

                if (Cache.Contains(pooledItem))
                {
                    pooledItem.OnRecycle();
                }
                pooledItem.OnDestroy();
            }
            DestroyItem((T)item);
        }

    }
}