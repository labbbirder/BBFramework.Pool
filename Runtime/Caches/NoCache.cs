using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BBFramework.Pool.Caches
{
    /// <summary>
    /// 不缓存任何元素的缓存
    /// </summary>
    public class NoCache : ICache
    {
        Dictionary<Type, int> outsideCount = new();

        IItemFactory ICache.itemFactory { get; set; }
        IItemFactory itemFactory => (this as ICache).itemFactory;


        void BumpCount(Type targetType, int v)
        {
            outsideCount[targetType] = outsideCount.GetValueOrDefault(targetType) + v;
        }
        public TSpec Rent<TSpec>()
        {
            BumpCount(typeof(TSpec), +1);

            var item = itemFactory.CreateItem<TSpec>();
            return item;
        }


        public void Return(object item)
        {
            BumpCount(item.GetType(), -1);
            itemFactory.DestroyItem(item);
        }


        public bool Contains(object item)
        {
            return false;
        }

        public void Clear(ClearReason reason)
        {
            outsideCount.Clear();
        }

        IEnumerator<(Type, int, int)> ICache.GetEnumerator()
        {
            foreach (var (type, cnt) in outsideCount)
            {
                yield return (type, 0, cnt);
            }
        }
    }
}