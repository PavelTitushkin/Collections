using System.Collections;

namespace Collections.CustomList
{
    public class CustomList<T> : IList<T>
    {
        private T[] _array;
        private int _count;
        static readonly T[] _emptyArray = new T[0];
        private readonly object _locker = new object();
        public CustomList()
        {
            _array = new T[4];
            _count = 0;
        }
        public CustomList(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (capacity == 0)
            {
                _array = _emptyArray;
            }
            else
            {
                _array = new T[capacity];
            }
        }

        public T this[int index]
        {
            get
            {
                lock (_locker)
                {
                    if (index < 0 || index >= _count)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    return _array[index];
                }
            }
            set
            {
                lock (_locker)
                {
                    if (index < 0 || index >= _count)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    _array[index] = value;
                }
            }
        }

        public int Count
        {
            get
            {
                lock (_locker)
                {
                    return _count;
                }
            }
        }

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            lock (_locker)
            {
                if (_count == _array.Length)
                {
                    T[] newArray = new T[_array.Length * 2];
                    Array.Copy(_array, newArray, _array.Length);
                    _array = newArray;
                }
                _array[_count++] = item;
            }
        }

        public void Clear()
        {
            lock (_locker)
            {
                if (_count > 0)
                {
                    _array = new T[4];
                    _count = 0;
                }
            }
        }

        public bool Contains(T item)
        {
            lock (_locker)
            {
                for (int i = 0; i < _count; i++)
                {
                    if (_array[i].Equals(item))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_locker)
            {
                Array.Copy(_array, 0, array, arrayIndex, _count);
            }
        }

        public int IndexOf(T item)
        {
            lock (_locker)
            {
                for (int i = 0; i < _count; i++)
                {
                    if (_array[i].Equals(item))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }

        public void Insert(int index, T item)
        {
            lock (_locker)
            {
                if (index < 0 || index > _count)
                {
                    throw new IndexOutOfRangeException();
                }
                if (_count == _array.Length)
                {
                    T[] newArray = new T[_array.Length * 2];
                    Array.Copy(_array, newArray, _array.Length);
                    _array = newArray;
                }
                Array.Copy(_array, index, _array, index + 1, _count - index);
                _array[index] = item;
                _count++;
            }
        }

        public bool Remove(T item)
        {
            lock (_locker)
            {
                int index = IndexOf(item);
                if (index == -1)
                {
                    return false;
                }
                RemoveAt(index);

                return true;
            }
        }

        public void RemoveAt(int index)
        {
            lock (_locker)
            {
                if (index < 0 || index >= _count)
                {
                    throw new IndexOutOfRangeException();
                }
                Array.Copy(_array, index + 1, _array, index, _count - index - 1);
                _count--;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_locker)
            {
                for (int i = 0; i < _count; i++)
                {
                    yield return _array[i];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
