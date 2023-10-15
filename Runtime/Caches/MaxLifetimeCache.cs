using System;
using System.Collections.Generic;
using UnityEngine;

namespace BBFramework.Pool.Caches
{
    using Timestamp = System.Single;
    struct ItemRecord
    {
        public object Value;
        public Timestamp recycleTime;

        public ItemRecord(object Value)
        {
            this.Value = Value;
            this.recycleTime = MaxLifetimeCache.Now;
        }
    }


    /// <summary>
    /// 元素计时销毁的缓存
    /// </summary>
    public class MaxLifetimeCache : CacheBase
    {
        internal static Timestamp Now => Time.unscaledTime;
        public Timestamp FloatingTTL = 60;
        public Timestamp PreferTTL = 60;
        Timestamp lastCollectTime;
        Dictionary<Type, LinkedList<ItemRecord>> pools;

        public MaxLifetimeCache() : this(60f)
        {
        }


        public MaxLifetimeCache(Timestamp maxLifetime)
        {
            pools = new();
            PreferTTL = FloatingTTL = maxLifetime;
        }


        public void CollectItems(bool force = false)
        {
            if (!force && lastCollectTime == Now) return;
            lastCollectTime = Now;

            // remove the outdated
            foreach (var (type, link) in pools)
            {
                while (link.Count > 0 && link.First.Value.recycleTime + FloatingTTL <= Now)
                {
                    var node = link.First;
                    link.RemoveFirst();
                    DestroyItem(node.Value.Value);
                }
            }
            FloatingTTL = PreferTTL * 0.2f + FloatingTTL * 0.8f;
        }


        LinkedList<ItemRecord> GetList(Type type)
        {
            if (!pools.TryGetValue(type, out var pool))
            {
                pools[type] = pool = new();
            }
            return pool;
        }


        public override T Rent<T>()
        {
            var type = typeof(T);
            var pool = GetList(type);
            T inst;
            if (pool.Count == 0)
            {
                inst = CreateItem<T>();
            }
            else
            {
                var node = pool.Last;
                pool.RemoveLast();
                inst = (T)node.Value.Value;
            }

            CollectItems();

            return inst;
        }


        public override void Return(object item)
        {
            var type = item.GetType();
            var list = GetList(type);

            var record = new ItemRecord(item);
            list.AddLast(record);

            CollectItems();
        }


        public override void Clear(ClearReason reason = ClearReason.Manual)
        {
            if (reason == ClearReason.LowMemory)
            {
                OnLowMemory();
            }
            else
            {
                ForceClearAll();
            }
        }


        void ForceClearAll()
        {
            foreach (var (t, list) in pools)
            {
                foreach (var item in list ?? new())
                {
                    DestroyItem(item.Value);
                }
                list.Clear();
            }
            pools.Clear();
        }


        /// <summary>
        /// 当收到内存警告时调用
        /// </summary>
        public void OnLowMemory()
        {
            if (pools.Count == 0) return;

            CollectItems();
            var minTimestamp = Timestamp.MaxValue;
            foreach (var (t, link) in pools)
            {
                if (link.Count == 0) continue;
                minTimestamp = Mathf.Min(minTimestamp, link.First.Value.recycleTime);
            }
            if (minTimestamp == Timestamp.MaxValue) return;

            FloatingTTL = Now - minTimestamp - 3;
            PreferTTL = Mathf.Max(0, PreferTTL * 0.8f + FloatingTTL * 0.2f);
            CollectItems();
        }
    }
}