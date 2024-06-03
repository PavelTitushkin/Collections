using System.Collections;

namespace Collections.CustomDictionary
{
    public class CustomDictionaryEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private readonly CustomDictionary<TKey, TValue> _dictionary;
        private int _index;
        private readonly int _version;
        private KeyValuePair<TKey, TValue> _current;
        private readonly object _locker = new object();

        public CustomDictionaryEnumerator(CustomDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
            _index = -1;
            _version = dictionary.GetVersion();
            _current = default;
        }

        public KeyValuePair<TKey, TValue> Current
        {
            get
            {
                lock (_locker)
                {
                    if (_index == -1 || _index >= _dictionary.Count)
                    {
                        throw new InvalidOperationException();
                    }
                    return _current;
                }
            }
        }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            lock (_locker)
            {
                if (_version != _dictionary.GetVersion())
                {
                    throw new InvalidOperationException();
                }

                while (++_index < _dictionary.Count)
                {
                    if (_dictionary.GetEntries()[_index].hashCode >= 0)
                    {
                        _current = new KeyValuePair<TKey, TValue>(_dictionary.GetEntries()[_index].key, _dictionary.GetEntries()[_index].value);
                        
                        return true;
                    }
                }

                _index = _dictionary.Count + 1;
                _current = default;

                return false;
            }
        }

        public void Reset()
        {
            lock (_locker)
            {
                if (_version != _dictionary.GetVersion())
                {
                    throw new InvalidOperationException();
                }

                _index = -1;
                _current = default;
            }
        }

        public void Dispose()
        {
            //
        }
    }
}
