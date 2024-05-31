using Collections.CustomDictionary;
using System.Collections;

namespace Collections.CustomList
{
    public class CustomList<T> : IList<T>
    {
        private T[] _array;
        private int _count;
        static readonly T[] _emptyArray = new T[0];
        public ReadWriteLocker _locker = new ReadWriteLocker();
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
                using (_locker.Read())
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
                using(_locker.Write())
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
                using(_locker.Read())
                {
                    return _count;
                }
            }
        }

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            using (_locker.Write())
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
            using (_locker.Write())
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
            using (_locker.Read())
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
            using (_locker.Read())
            {
                Array.Copy(_array, 0, array, arrayIndex, _count);
            }
        }

        public int IndexOf(T item)
        {
            using (_locker.Read())
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
            using (_locker.Write())
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
            using (_locker.Write())
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
            using (_locker.Write())
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
            using (_locker.Read())
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
