

using System;
using System.Collections.Generic;

namespace BBFramework.Pool
{
    public abstract class CacheBase : ICache
    {
        IItemFactory ICache.itemFactory { get; set; }
        Dictionary<Type, (int, int)> itemCounter;
        HashSet<object> items;

        public CacheBase()
        {
            itemCounter = new();
            items = new();
        }

        void BumpRecord(Type targetType, int pooledCount = 0, int outsideCount = 0)
        {
            if (!itemCounter.TryGetValue(targetType, out var rec))
            {
                itemCounter[targetType] = rec = (0, 0);
            }
            var (a, b) = rec;
            itemCounter[targetType] = (a + pooledCount, b + outsideCount);
        }


        protected virtual T CreateItem<T>()
        {
            var targetType = typeof(T);
            var item = (this as ICache).itemFactory.CreateItem<T>();
            BumpRecord(targetType, pooledCount: +1);
            return item;
        }


        protected virtual void DestroyItem(object item)
        {
            if (item is null)
            {
                throw new ArgumentException($"item cannot be null");
            }
            if (!items.Contains(item))
            {
                throw new ArgumentException($"only pooled item can be destroyed");
            }

            var targetType = item.GetType();
            ; (this as ICache).itemFactory.DestroyItem(item);
            BumpRecord(targetType, pooledCount: -1); // can only destroy pooled
            items.Remove(item);
        }

        bool ICache.Contains(object item)
        {
            if (item is null)
            {
                throw new ArgumentException($"item cannot be null");
            }
            return items.Contains(item);
        }

        T ICache.Rent<T>()
        {
            var targetType = typeof(T);
            var item = Rent<T>();
            if (item != null)
            {
                BumpRecord(targetType, pooledCount: -1, outsideCount: +1);
                items.Remove(item);
            }
            return item;
        }


        void ICache.Return(object item)
        {
            if (item is null)
            {
                throw new ArgumentException($"item cannot be null");
            }
            items.Add(item);
            var targetType = item.GetType();
            try { Return(item); }
            catch { throw; }
            finally
            {
                BumpRecord(targetType, pooledCount: +1, outsideCount: -1);
            }
        }

        IEnumerator<(Type, int, int)> ICache.GetEnumerator()
        {
            foreach (var (type, countPair) in itemCounter)
            {
                var (a, b) = countPair;
                yield return (type, a, b);
            }
        }

        public abstract T Rent<T>();
        public abstract void Return(object item);
        public abstract void Clear(ClearReason reason = ClearReason.Manual);
    }
}