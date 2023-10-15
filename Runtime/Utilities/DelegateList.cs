// author: bbbirder

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BBFramework.Events
{
    /// <summary>
    /// 更高效的回调类
    /// </summary>
    public abstract class DelegateListBase<T> where T: Delegate
    {
        protected LinkedList<T> delegates;
        Dictionary<Delegate,LinkedListNode<T>> lut;
        internal DelegateListBase()
        {
            lut = new();
            delegates = new();
        }

        public void Add(T keyDelegate)
        {
            if (lut.ContainsKey(keyDelegate)) return;
            lut[keyDelegate] = delegates.AddLast(keyDelegate);
        }

        public void Add(Delegate keyDelegate, T valueDelegate)
        {
            if (lut.TryGetValue(keyDelegate, out var node))
            {
                delegates.Remove(node);
            }
            lut[keyDelegate] = delegates.AddLast(valueDelegate);
        }

        public bool Remove(Delegate keyDelegate)
        {
            if (lut.TryGetValue(keyDelegate, out var node))
            {
                delegates.Remove(node);
                lut.Remove(keyDelegate);
                return true;
            }
            return false;
        }

        public void Clear(){
            lut.Clear();
            delegates.Clear();
        }
    }


    /// <inheritdoc/>
    public class DelegateList : DelegateListBase<Action>
    {
        [HideInCallstack]
        public void Invoke()
        {
            foreach(var d in delegates){
                d.Invoke();
            }
        }
    }


    /// <inheritdoc/>
    public class DelegateList<T> : DelegateListBase<Action<T>>
    {
        [HideInCallstack]
        public void Invoke(T arg)
        {
            foreach(var d in delegates){
                d.Invoke(arg);
            }
        }
    }
}