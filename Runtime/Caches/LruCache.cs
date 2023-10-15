
using System;
using System.Collections.Generic;

namespace BBFramework.Pool.Caches
{
    using Node = LinkedListNode<object>;
    using NodeList = LinkedList<object>;
    /// <summary>
    /// LRU算法的缓存
    /// </summary>
    class LruCache : CacheBase
    {
        public int Capacity;
        private readonly NodeList container;
        private readonly Dictionary<Type, List<Node>> lutItems;

        public LruCache() : this(32) { }
        public LruCache(int Capacity)
        {
            this.Capacity = Capacity;
            this.container = new();
            this.lutItems = new();
        }


        List<Node> GetItems(Type type)
        {
            if (!lutItems.TryGetValue(type, out var items))
            {
                lutItems[type] = items = new();
            }
            return items;
        }


        public override void Clear(ClearReason reason = ClearReason.Manual)
        {
            foreach(var node in container){
                DestroyItem(node);
            }
            container.Clear();
            lutItems.Clear();
        }


        public override T Rent<T>()
        {
            var targetType = typeof(T);
            var items = GetItems(targetType);
            if (items.Count > 0)
            {
                var item = items[^1];
                items.RemoveAt(items.Count - 1);
                container.Remove(item);
                return (T)item.Value;
            }
            return CreateItem<T>();
        }


        public override void Return(object item)
        {
            var targetType = item.GetType();
            var items = GetItems(targetType);

            var node = container.AddFirst(item);
            items.Add(node);
            while (container.Count > Capacity)
            {
                var lastNode = container.Last;
                DestroyItem(lastNode.Value);
                container.RemoveLast();
                items.Remove(lastNode);
            }
        }
    }
}