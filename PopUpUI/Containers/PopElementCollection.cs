using PopUpUI.Containers;
using PopUpUI.Elements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PopUpUI.Containers
{
    public class PopElementCollection : ICollection<PopElement>
    {
        private readonly RepeatableSortedList<int, PopElement> _sortedElements;

        bool ICollection<PopElement>.IsReadOnly => false;

        public int Count => _sortedElements.Count;
        public int Capacity { get => _sortedElements.Capacity; set => _sortedElements.Capacity = value; }

        public PopElementCollection()
        {
            _sortedElements = new RepeatableSortedList<int, PopElement>();
        }

        public void Add(PopElement element)
        {
            _sortedElements.Add(_sortedElements.Max.Key + 1, element);
        }

        public void Add(PopElement element, int zIndex)
        {
            _sortedElements.Add(zIndex, element);
        }

        public void AddRange(IEnumerable<PopElement> collection)
        {
            ArgumentNullException.ThrowIfNull(collection);

            foreach (var element in collection) Add(element);
        }

        public void AddRange(IEnumerable<PopElement> collection, int startZIndex)
        {
            ArgumentNullException.ThrowIfNull(collection);

            foreach (var element in collection) Add(element, startZIndex++);
        }

        public bool Remove(PopElement element)
        {
            int index = _sortedElements.IndexOfValue(element);
            if (index >= 0)
            {
                _sortedElements.RemoveAt(index);
                return true;
            }

            return false;
        }

        public int GetZIndex(PopElement element)
        {
            return _sortedElements.GetKeyAtIndex(_sortedElements.IndexOfValue(element));
        }

        public void SetZIndex(PopElement element, int zIndex)
        {
            int indexInsideCollection = _sortedElements.IndexOfValue(element);
            if (indexInsideCollection < 0) throw new ArgumentException("PopUpElement is not presented inside PopUpElementCollection", nameof(element));

            _sortedElements.RemoveAt(indexInsideCollection);
            _sortedElements.Add(zIndex, element);
        }

        public void Clear()
        {
            _sortedElements.Clear();
        }

        public void TrimExcess()
        {
            _sortedElements.TrimExcess();
        }

        public bool Contains(PopElement element)
        {
            return _sortedElements.ContainsValue(element);
        }

        public void CopyTo(PopElement[] array, int arrayIndex)
        {
            ArgumentNullException.ThrowIfNull(array);

            if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException();

            if (array.Length - arrayIndex < Count) throw new ArgumentException();

            foreach (var element in _sortedElements.Values)
            {
                array[arrayIndex++] = element;
            }
        }

        public IEnumerator<PopElement> GetEnumerator()
        {
            return _sortedElements.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal IEnumerable<T> WhereWithType<T>(Predicate<T> match)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (_sortedElements.Values[i].Visibility != Visibility.Hidden &&
                    _sortedElements.Values[i] is T instance && match(instance))
                {
                    yield return instance;
                }
            }
        }

#pragma warning disable 8632

        internal T? FindWithType<T>(Predicate<T> match)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (_sortedElements.Values[i].Visibility != Visibility.Hidden && 
                    _sortedElements.Values[i] is T instance && match(instance))
                {
                    return instance;
                }
            }

            return default;
        }

#pragma warning restore 8632
    }
}
