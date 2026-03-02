using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#nullable enable

namespace PopUpUI.Containers
{
    /// <summary>Represents a sorted collection that can have duplicate <typeparamref name="TKey"/> entries.</summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class RepeatableSortedList<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
    {
        private TKey[] keys;
        private TValue[] values;
        private int _size;
        private KeyList? keyList;
        private ValueList? valueList;

        private const int DefaultCapacity = 4;

        public int Count => _size;
        public IComparer<TKey> Comparer { get; }
        public KeyList Keys => keyList ??= new KeyList(this);
        public ValueList Values => valueList ??= new ValueList(this);

        public KeyValuePair<TKey, TValue> Min
        {
            get
            {
                if (_size == 0) return default;

                return new KeyValuePair<TKey, TValue>(keys[0], values[0]);
            }
        }
        public KeyValuePair<TKey, TValue> Max
        {
            get
            {
                if (_size == 0) return default;

                return new KeyValuePair<TKey, TValue>(keys[Count - 1], values[Count - 1]);
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

        public int Capacity
        {
            get => keys.Length;
            set
            {
                if (value != keys.Length)
                {
                    ArgumentOutOfRangeException.ThrowIfLessThan(value, _size);

                    if (value > 0)
                    {
                        TKey[] newKeys = new TKey[value];
                        TValue[] newValues = new TValue[value];
                        if (_size > 0)
                        {
                            Array.Copy(keys, newKeys, _size);
                            Array.Copy(values, newValues, _size);
                        }
                        keys = newKeys;
                        values = newValues;
                    }
                    else
                    {
                        keys = Array.Empty<TKey>();
                        values = Array.Empty<TValue>();
                    }
                }
            }
        }

        public RepeatableSortedList()
        {
            keys = Array.Empty<TKey>();
            values = Array.Empty<TValue>();
            _size = 0;
            Comparer = Comparer<TKey>.Default;
        }

        public RepeatableSortedList(int capacity)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(capacity);
            keys = new TKey[capacity];
            values = new TValue[capacity];
            Comparer = Comparer<TKey>.Default;
        }

        public RepeatableSortedList(IComparer<TKey>? comparer) : this()
        {
            if (comparer != null) Comparer = comparer;
        }

        public RepeatableSortedList(int capacity, IComparer<TKey>? comparer) : this(comparer)
        {
            Capacity = capacity;
        }

        public RepeatableSortedList(IDictionary<TKey, TValue> dictionary) : this(dictionary, null)
        {
        }

        public RepeatableSortedList(IDictionary<TKey, TValue> dictionary, IComparer<TKey>? comparer) 
            : this(dictionary?.Count ?? throw new ArgumentNullException(), comparer)
        {
            int count = dictionary.Count;
            if (count != 0)
            {
                dictionary.Keys.CopyTo(keys, 0);
                dictionary.Values.CopyTo(values, 0);

                if (count > 1)
                {
                    comparer = Comparer; // obtain default if this is null.
                    Array.Sort(keys, values, comparer);
                }
            }

            _size = count;
        }

        public TValue this[TKey key]
        {
            get
            {
                int i = IndexOfKey(key);
                if (i >= 0)
                    return values[i];

                throw new KeyNotFoundException();
            }
            set
            {
                if (key == null) throw new ArgumentNullException(nameof(key));

                int i = Array.BinarySearch(keys, 0, _size, key, Comparer);
                if (i >= 0)
                {
                    values[i] = value;
                    return;
                }

                throw new KeyNotFoundException();
            }
        }

        public void Add(TKey key, TValue value)
        {
            ArgumentNullException.ThrowIfNull(key);

            int i = Array.BinarySearch(keys, 0, _size, key, Comparer);
            if (i > 0)
            {
                Insert(i, key, value);
            }
            else
            {
                Insert(~i == -1 ? 0 : ~i, key, value);
            }
        }

        public bool Remove(TKey key)
        {
            int i = IndexOfKey(key);
            if (i >= 0) RemoveAt(i);

            return i >= 0;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _size) throw new ArgumentOutOfRangeException();
            _size--;

            if (index < _size)
            {
                Array.Copy(keys, index + 1, keys, index, _size - index);
                Array.Copy(values, index + 1, values, index, _size - index);
            }
            if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
            {
                keys[_size] = default!;
            }
            if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
            {
                values[_size] = default!;
            }
        }

        public bool RemovePairWithValue(TValue value)
        {
            int i = IndexOfValue(value);
            if (i >= 0) RemoveAt(i);

            return i >= 0;
        }

        public void Clear()
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
            {
                Array.Clear(keys, 0, _size);
            }
            if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
            {
                Array.Clear(values, 0, _size);
            }
            _size = 0;
        }

        public int IndexOfKey(TKey key)
        {
            ArgumentNullException.ThrowIfNull(key);

            int ret = Array.BinarySearch(keys, 0, _size, key, Comparer);
            return ret >= 0 ? ret : -1;
        }

        public int IndexOfValue(TValue value)
        {
            return Array.IndexOf(values, value, 0, _size);
        }

        public TKey GetKeyAtIndex(int index)
        {
            if (index < 0 || index >= _size) throw new IndexOutOfRangeException();

            return keys[index];
        }

        public void SetValueAtIndex(int index, TValue value)
        {
            if (index < 0 || index >= _size) throw new IndexOutOfRangeException();

            values[index] = value;
        }

        public TValue GetValueAtIndex(int index)
        {
            if (index < 0 || index >= _size) throw new IndexOutOfRangeException();

            return values[index];
        }

        public void TrimExcess()
        {
            int threshold = (int)(keys.Length * 0.9);
            if (_size < threshold)
            {
                Capacity = _size;
            }
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            int i = IndexOfKey(key);
            if (i >= 0)
            {
                value = values[i];
                return true;
            }

            value = default;
            return false;
        }

        public bool ContainsKey(TKey key)
        {
            return IndexOfKey(key) >= 0;
        }

        public bool ContainsValue(TValue value)
        {
            return IndexOfValue(value) >= 0;
        }

        public TKey? FindKey(Predicate<TKey> match)
        {
            ArgumentNullException.ThrowIfNull(match);

            for (int i = 0; i < _size; i++)
            {
                if (match(keys[i])) return keys[i];
            }

            return default;
        }

        public TKey? FindLastKey(Predicate<TKey> match)
        {
            ArgumentNullException.ThrowIfNull(match);

            for (int i = _size - 1; i >= 0; i--)
            {
                if (match(keys[i])) return keys[i];
            }

            return default;
        }

        public TValue? FindValue(Predicate<TValue> match)
        {
            ArgumentNullException.ThrowIfNull(match);

            for (int i = 0; i < _size; i++)
            {
                if (match(values[i])) return values[i];
            }

            return default;
        }

        public TValue? FindLastValue(Predicate<TValue> match)
        {
            ArgumentNullException.ThrowIfNull(match);

            for (int i = _size - 1; i >= 0; i--)
            {
                if (match(values[i])) return values[i];
            }

            return default;
        }

        private void Insert(int index, TKey key, TValue value)
        {
            if (_size == keys.Length) EnsureCapacity(_size + 1);
            if (index < _size)
            {
                Array.Copy(keys, index, keys, index + 1, _size - index);
                Array.Copy(values, index, values, index + 1, _size - index);
            }
            keys[index] = key;
            values[index] = value;
            _size++;
        }

        private void EnsureCapacity(int min)
        {
            int newCapacity = keys.Length == 0 ? DefaultCapacity : keys.Length * 2;
            // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
            if (newCapacity < min) newCapacity = min;
            Capacity = newCapacity;
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            int index = IndexOfKey(item.Key);

            return index >= 0 && EqualityComparer<TValue>.Default.Equals(values[index], item.Value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ArgumentNullException.ThrowIfNull(array);

            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (array.Length - arrayIndex < Count)
            {
                throw new ArgumentException();
            }

            for (int i = 0; i < Count; i++)
            {
                KeyValuePair<TKey, TValue> entry = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
                array[arrayIndex + i] = entry;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = 0; i < _size; i++)
            {
                yield return new KeyValuePair<TKey, TValue>(keys[i], values[i]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public class KeyList : IList<TKey>, ICollection<TKey>
        {
            private readonly RepeatableSortedList<TKey, TValue> list;

            public int Count => list._size;
            public bool IsReadOnly => true;

            internal KeyList(RepeatableSortedList<TKey, TValue> list)
            {
                this.list = list;
            }

            public TKey this[int index] 
            {
                get => list.GetKeyAtIndex(index);
                set => throw new NotSupportedException();
            }

            public void Add(TKey item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TKey item)
            {
                return list.ContainsKey(item);
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                Array.Copy(list.keys, 0, array, arrayIndex, list.Count);
            }

            public int IndexOf(TKey item)
            {
                return list.IndexOfKey(item);
            }

            public void Insert(int index, TKey item)
            {
                throw new NotSupportedException();
            }

            public bool Remove(TKey item)
            {
                throw new NotSupportedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<TKey> GetEnumerator()
            {
                for (int i = 0; i < list._size; i++)
                {
                    yield return list.keys[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class ValueList : IList<TValue>, ICollection<TValue>
        {
            private readonly RepeatableSortedList<TKey, TValue> list;

            public int Count => list._size;
            public bool IsReadOnly => true;

            internal ValueList(RepeatableSortedList<TKey, TValue> list)
            {
                this.list = list;
            }

            public TValue this[int index] 
            {
                get => list.GetValueAtIndex(index);
                set => throw new NotSupportedException();
            }

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
                return list.ContainsValue(item);
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                Array.Copy(list.values, 0, array, arrayIndex, list.Count);
            }

            public int IndexOf(TValue item)
            {
                return list.IndexOfValue(item);
            }

            public void Insert(int index, TValue item)
            {
                throw new NotSupportedException();
            }

            public bool Remove(TValue item)
            {
                throw new NotSupportedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<TValue> GetEnumerator()
            {
                for (int i = 0; i < list._size; i++)
                {
                    yield return list.values[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
