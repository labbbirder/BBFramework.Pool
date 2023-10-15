using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BBFramework.Pool.Caches
{
    /// <summary>
    /// 不积极淘汰元素的缓存
    /// </summary>
    public class GreedCache : CacheBase
    {
        Dictionary<Type, HashSet<object>> container = new();


        HashSet<object> GetItems(Type type)
        {
            if (!container.TryGetValue(type, out var items))
            {
                container[type] = items = new();
            }
            return items;
        }


        void CheckNotNull(object inst)
        {
            if (inst is null)
                throw new ArgumentException($"cannot recycle null. Pool: {this}");
        }


        public override T Rent<T>()
        {
            var targetType = typeof(T);
            var items = GetItems(targetType);

            var inst = items.FirstOrDefault();
            if (inst != null)
            {
                items.Remove(inst);
                return (T)inst;
            }

            return CreateItem<T>();
        }


        public override void Return(object item)
        {
            CheckNotNull(item);

            var targetType = item.GetType();
            var items = GetItems(targetType);

            items.Add(item);
        }


        public override void Clear(ClearReason reason = ClearReason.Manual)
        {
            if (reason == ClearReason.Manual)
            {
                ClearForManual();
            }
            else if (reason == ClearReason.LowMemory)
            {
                ClearForLowMemory();
            }
            else
            {
                // Assert: not reachable here ...
            }
        }


        public void ClearForManual()
        {
            foreach (var (_, items) in container)
            {
                foreach (var item in items ?? new())
                {
                    DestroyItem(item);
                }
                items?.Clear();
            }
            container.Clear();
        }


        public void ClearForLowMemory()
        {
            ClearForManual();
        }

    }
}