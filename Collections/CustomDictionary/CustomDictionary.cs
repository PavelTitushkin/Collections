using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Collections.CustomDictionary
{
    public class CustomDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private struct Entry
        {
            public int hashCode;
            public int next;
            public TKey key;
            public TValue value;
        }
        private int[] buckets;
        private Entry[] entries;
        private int count;
        private int version;
        private int freeList;
        private int freeCount;
        private IEqualityComparer<TKey> comparer;
        private KeyCollection keys;
        private ValueCollection values;
        private ReadWriteLocker locker = new ReadWriteLocker();

        public CustomDictionary() : this(0, null) { }

        public CustomDictionary(int capacity) : this(capacity, null) { }

        public CustomDictionary(IEqualityComparer<TKey> comparer) : this(0, comparer) { }

        public CustomDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (capacity > 0)
            {
                Initialize(capacity);
            }
            this.comparer = comparer ?? EqualityComparer<TKey>.Default;
        }

        public TValue this[TKey key]
        {
            get
            {
                using (locker.Read())
                {
                    int i = FindEntry(key);
                    if (i >= 0)
                    {
                        return entries[i].value;
                    }

                    return default(TValue);
                }
            }
            set
            {
                using(locker.Write())
                {
                    Insert(key, value, false);
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                using (locker.Read())
                {
                    Contract.Ensures(Contract.Result<KeyCollection>() != null);
                    if (keys == null)
                    {
                        keys = new KeyCollection(this);
                    }

                    return keys;
                }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                using (locker.Read())
                {
                    Contract.Ensures(Contract.Result<ValueCollection>() != null);
                    if (values == null)
                    {
                        values = new ValueCollection(this);
                    }

                    return values;
                }
            }
        }

        public int Count
        {
            get 
            {
                using (locker.Read())
                {
                    return count - freeCount;
                }
            }
        }

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            using (locker.Write())
            {
                Insert(key, value, true);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            using (locker.Write())
            {
                Add(item.Key, item.Value);
            }
        }

        public void Clear()
        {
            using (locker.Write())
            {
                if (count > 0)
                {
                    for (int i = 0; i < buckets.Length; i++)
                    {
                        buckets[i] = -1;
                    }
                    Array.Clear(entries, 0, count);
                    freeList = -1;
                    count = 0;
                    freeCount = 0;
                    version++;
                }
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            using (locker.Read())
            {
                int i = FindEntry(item.Key);
                if (i >= 0 && EqualityComparer<TValue>.Default.Equals(entries[i].value, item.Value))
                {
                    return true;
                }

                return false;
            }
        }

        public bool ContainsKey(TKey key)
        {
            using (locker.Read())
            {
                return FindEntry(key) >= 0;
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException();
            }
            using (locker.Write())
            {
                if (buckets != null)
                {
                    int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
                    int bucket = hashCode % buckets.Length;
                    int last = -1;
                    for (int i = buckets[bucket]; i >= 0; last = i, i = entries[i].next)
                    {
                        if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
                        {
                            if (last < 0)
                            {
                                buckets[bucket] = entries[i].next;
                            }
                            else
                            {
                                entries[last].next = entries[i].next;
                            }
                            entries[i].hashCode = -1;
                            entries[i].next = freeList;
                            entries[i].key = default(TKey);
                            entries[i].value = default(TValue);
                            freeList = i;
                            freeCount++;
                            version++;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            using (locker.Write())
            {
                int i = FindEntry(item.Key);
                if (i >= 0 && EqualityComparer<TValue>.Default.Equals(entries[i].value, item.Value))
                {
                    Remove(item.Key);

                    return true;
                }

                return false;
            }
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            using (locker.Read())
            {
                int i = FindEntry(key);
                if (i >= 0)
                {
                    value = entries[i].value;
                    return true;
                }
                value = default(TValue);

                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            using(locker.Read())
            {
                for (int i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0)
                    {
                        yield return new KeyValuePair<TKey, TValue>(entries[i].key, entries[i].value);
                    }
                }
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
           using(locker.Read())
            {
                for (int i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0)
                    {
                        yield return new KeyValuePair<TKey, TValue>(entries[i].key, entries[i].value);
                    }
                }
            }
        }

        private int FindEntry(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException();
            }

            if (buckets != null)
            {
                int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
                for (int i = buckets[hashCode % buckets.Length]; i >= 0; i = entries[i].next)
                {
                    if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private void Initialize(int capacity)
        {
            int size = HashHelpers.GetPrime(capacity);

            buckets = new int[size];
            for (int i = 0; i < buckets.Length; i++)
            {
                buckets[i] = -1;
            }

            entries = new Entry[size];
            freeList = -1;
        }

        private void Insert(TKey key, TValue value, bool add)
        {

            if (key == null)
            {
                throw new ArgumentNullException();
            }
            if (buckets == null || buckets.Length == 0)
            {
                Initialize(0);
            }

            int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
            int targetBucket = hashCode % buckets.Length;

            for (int i = buckets[targetBucket]; i >= 0; i = entries[i].next)
            {
                if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
                {
                    if (add)
                    {
                        throw new ArgumentException();
                    }
                    entries[i].value = value;
                    version++;
                    return;
                }
            }

            int index;
            if (freeCount > 0)
            {
                index = freeList;
                freeList = entries[index].next;
                freeCount--;
            }
            else
            {
                if (count == entries.Length)
                {
                    Resize();
                    targetBucket = hashCode % buckets.Length;
                }
                index = count;
                count++;
            }

            entries[index].hashCode = hashCode;
            entries[index].next = buckets[targetBucket];
            entries[index].key = key;
            entries[index].value = value;
            buckets[targetBucket] = index;
            version++;
        }

        private void Resize()
        {
            int newSize = HashHelpers.GetPrime(count * 2);

            int[] newBuckets = new int[newSize];
            for (int i = 0; i < newBuckets.Length; i++)
            {
                newBuckets[i] = -1;
            }

            Entry[] newEntries = new Entry[newSize];
            Array.Copy(entries, 0, newEntries, 0, count);

            for (int i = 0; i < count; i++)
            {
                int bucket = newEntries[i].hashCode % newSize;
                newEntries[i].next = newBuckets[bucket];
                newBuckets[bucket] = i;
            }

            buckets = newBuckets;
            entries = newEntries;
        }

        public sealed class KeyCollection : ICollection<TKey>
        {
            private CustomDictionary<TKey, TValue> _dictionary;
            public KeyCollection(CustomDictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                {
                    throw new ArgumentNullException();
                }

                this._dictionary = dictionary;
            }

            public int Count => _dictionary.Count;


            public bool IsReadOnly => false;

            public void Add(TKey item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                _dictionary.Clear();
            }

            public bool Contains(TKey item)
            {
                return _dictionary.ContainsKey(item);
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                if (array == null)
                {
                    throw new ArgumentNullException();
                }

                foreach (TKey key in this)
                {
                    array[arrayIndex++] = key;
                }
            }

            public bool Remove(TKey item)
            {
                throw new NotSupportedException();
            }
            public IEnumerator<TKey> GetEnumerator()
            {
                foreach (KeyValuePair<TKey, TValue> pair in _dictionary)
                {
                    yield return pair.Key;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public sealed class ValueCollection : ICollection<TValue>
        {
            private CustomDictionary<TKey, TValue> _dictionary;

            public ValueCollection(CustomDictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                {
                    throw new ArgumentNullException();
                }
                this._dictionary = dictionary;
            }
            public int Count => _dictionary.Count;

            public bool IsReadOnly => false;

            public void Add(TValue item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TValue item)
            {
                throw new NotSupportedException();
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                throw new NotSupportedException();
            }

            public bool Remove(TValue item)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<TValue> GetEnumerator()
            {
                foreach (KeyValuePair<TKey, TValue> pair in _dictionary)
                {
                    yield return pair.Value;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
