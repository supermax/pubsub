using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SuperMaxim.Core.Collections
{
    internal class ThreadSafeDic<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _dic = new Dictionary<TKey, TValue>();

        public TValue this[TKey key] { get => _dic[key]; set => _dic[key] = value; }

        public ICollection<TKey> Keys => ((IDictionary<TKey, TValue>)_dic).Keys;

        public ICollection<TValue> Values => ((IDictionary<TKey, TValue>)_dic).Values;

        public int Count => _dic.Count;

        public bool IsReadOnly => ((IDictionary<TKey, TValue>)_dic).IsReadOnly;

        public void Add(TKey key, TValue value)
        {
            lock (_dic)
            {
                _dic.Add(key, value);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (_dic)
            {
                ((IDictionary<TKey, TValue>)_dic).Add(item);
            }
        }

        public void Clear()
        {
            _dic.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((IDictionary<TKey, TValue>)_dic).Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return _dic.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary<TKey, TValue>)_dic).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ((IDictionary<TKey, TValue>)_dic).GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            return _dic.Remove(key);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((IDictionary<TKey, TValue>)_dic).Remove(item);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dic.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<TKey, TValue>)_dic).GetEnumerator();
        }
    }
}
