

using System;
using System.Collections.Generic;

namespace BBFramework.Pool.Caches
{
    /// <summary>
    /// 使用弱引用保存对象的缓存（GC销毁）
    /// </summary>
    class WeakCache : CacheBase
    {
        public Dictionary<Type, List<WeakReference>> container;
        public WeakCache()
        {
            container = new();
        }
        public List<WeakReference> GetItems(Type type)
        {
            if (!container.TryGetValue(type, out var items))
            {
                container[type] = items = new();
            }
            return items;
        }

        public override void Clear(ClearReason reason = ClearReason.Manual)
        {
            foreach (var (type, items) in container)
            {
                foreach (var item in items)
                {
                    DestroyItem(item.Target);
                }
                items.Clear();
            }
            container.Clear();
        }


        public override T Rent<T>()
        {
            var items = GetItems(typeof(T));

            WeakReference wr = default;
            while (items.Count > 0)
            {
                wr = items[0];
                if (!wr.IsAlive || wr.Target is null)
                {
                    items[0] = items[^1];
                    items.RemoveAt(items.Count - 1);
                    DestroyItem(wr.Target);
                }
                else
                {
                    break;
                }
            }
            if (items.Count == 0)
            {
                wr = new WeakReference(CreateItem<T>());
                items.Add(wr);
            }
            return (T)wr.Target;
        }

        public override void Return(object item)
        {
            var items = GetItems(item.GetType());
            items.Add(new WeakReference(item));
        }
    }
}