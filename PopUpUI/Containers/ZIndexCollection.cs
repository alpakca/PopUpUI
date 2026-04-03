using System;
using System.Collections;
using System.Collections.Generic;

#nullable enable

namespace PopUpUI.Containers
{
    /// <summary>Represents a collection that can sort generic elements through their Z-Indexes.</summary>
    /// <typeparam name="T"></typeparam>
    public class ZIndexCollection<T> : ICollection<T>
    {
        private readonly RepeatableSortedList<int, T> _sortedElements;

        protected RepeatableSortedList<int, T> SortedElements => _sortedElements;

        bool ICollection<T>.IsReadOnly => false;

        /// <summary>Gets the count of <see cref="T"/>s this collection contains.</summary>
        public int Count => _sortedElements.Count;
        /// <summary>Gets or sets the total number of elements collection can hold without resizing.</summary>
        public int Capacity
        {
            get => _sortedElements.Capacity;
            set => _sortedElements.Capacity = value;
        }

        /// <summary>Gets the element that is at bottom in the collection.</summary>
        public T? Bottom => Count == 0 ? default : _sortedElements.Min.Value;
        /// <summary>Gets the Z-Index of element that is at bottom in the collection.</summary>
        public int BottomIndex => Count == 0 ? default : _sortedElements.Min.Key;

        /// <summary>Gets the element that is top most in the collection.</summary>
        public T? Top => Count == 0 ? default : _sortedElements.Max.Value;
        /// <summary>Gets the Z-Index of element that is top most in the collection.</summary>
        public int TopIndex => Count == 0 ? default : _sortedElements.Max.Key;

        /// <summary>Initializes a new instance of <see cref="ZIndexCollection{T}"/> class with Z-Indexes in an ascending order..</summary>
        public ZIndexCollection()
        {
            _sortedElements = new RepeatableSortedList<int, T>();
        }

        /// <summary>Initializes a new instance of <see cref="ZIndexCollection{T}"/> class with a comparer for Z-Indexes.</summary>
        /// <param name="zIndexComparer">Comparer for the Z-Indexes.</param>
        public ZIndexCollection(IComparer<int>? zIndexComparer)
        {
            _sortedElements = new RepeatableSortedList<int, T>(zIndexComparer);
        }

        /// <summary>Initializes a new instance of <see cref="ZIndexCollection{T}"/> class with elements copied from <paramref name="collection"/>.</summary>
        /// <param name="collection">Collection to copy elements from.</param>
        public ZIndexCollection(IEnumerable<T> collection) : this(collection, null)
        {
        }

        /// <summary>Initializes a new instance of <see cref="ZIndexCollection{T}"/> class 
        /// with elements copied from <paramref name="collection"/> and a comparer for Z-Indexes.</summary>
        /// <param name="collection">Collection to copy elements from.</param>
        /// <param name="zIndexComparer">Comparer for the Z-Indexes.</param>
        public ZIndexCollection(IEnumerable<T> collection, IComparer<int>? zIndexComparer) : this(zIndexComparer)
        {
            AddRange(collection);
        }

        /// <summary>Adds an element to collection and places it up most section in collection.</summary>
        /// <param name="element">Element to add.</param>
        public virtual void Add(T element)
        {
            _sortedElements.Add(TopIndex + 1, element);
        }

        /// <summary>Adds an element to collection with specified <paramref name="zIndex"/>.</summary>
        /// <param name="element">Element to add.</param>
        /// <param name="zIndex">The ZIndex of the element inside collection.</param>
        public virtual void Add(T element, int zIndex)
        {
            _sortedElements.Add(zIndex, element);
        }

        /// <summary>Adds a range of elements to collection with a ZIndex greater than all elements starting from first element of the range.</summary>
        /// <param name="collection">Elements to add to collection.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual void AddRange(IEnumerable<T> collection)
        {
            ArgumentNullException.ThrowIfNull(collection);

            foreach (var element in collection) Add(element);
        }

        /// <summary>Adds a range of elements to collection with their ZIndex starting from <paramref name="zIndexStart"/>.</summary>
        /// <param name="collection">Elements to add to collection.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual void AddRange(IEnumerable<T> collection, int zIndexStart)
        {
            ArgumentNullException.ThrowIfNull(collection);

            foreach (var element in collection) Add(element, zIndexStart++);
        }

        /// <summary>Removes the given element from collection.</summary>
        /// <param name="element">Element to remove from collection.</param>
        /// <returns><see langword="true"/> if element was successfully removed; otherwise <see langword="false"/>. 
        /// This method also returns <see langword="false"/> if element was not found in the collection.</returns>
        public virtual bool Remove(T element)
        {
            int index = _sortedElements.IndexOfValue(element);
            if (index >= 0)
            {
                _sortedElements.RemoveAt(index);
            }

            return index >= 0;
        }

        /// <summary>Clears all of the elements from collection.</summary>
        public virtual void Clear()
        {
            _sortedElements.Clear();
        }

        /// <summary>Sets the capacity to the actual number of elements in collection, if that number is less than a threshold value.</summary>
        public void TrimExcess()
        {
            _sortedElements.TrimExcess();
        }

        /// <summary>Determines if <paramref name="element"/> is presented inside collection.</summary>
        /// <param name="element">The item to look for.</param>
        /// <returns><see langword="true"/> if <paramref name="element"/> is found in the collection; otherwise <see langword="false"/>.</returns>
        public bool Contains(T element)
        {
            return _sortedElements.ContainsValue(element);
        }

        /// <summary>Copies the elements of the collection to given array, starting from given index.</summary>
        /// <param name="array">Array to copy elements to.</param>
        /// <param name="arrayIndex">The index to start copying from.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            ArgumentNullException.ThrowIfNull(array);

            if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException();

            if (array.Length - arrayIndex < Count) throw new ArgumentException();

            for (int i = 0; i < _sortedElements.Values.Count; i++)
            {
                array[arrayIndex + i] = _sortedElements.Values[i];
            }
        }

        /// <summary>Gets the ZIndex of the <paramref name="element"/>.</summary>
        /// <param name="element">Element to get ZIndex of.</param>
        /// <returns>ZIndex of the element.</returns>
        /// <exception cref="ArgumentException"></exception>
        public int GetZIndex(T element)
        {
            return _sortedElements.GetKeyAtIndex(_sortedElements.IndexOfValue(element));
        }

        /// <summary>Sets the ZIndex of the <paramref name="element"/>.</summary>
        /// <param name="element">Element to set ZIndex of.</param>
        /// <param name="zIndex">The new ZIndex of the element.</param>
        /// <exception cref="ArgumentException"></exception>
        public void SetZIndex(T element, int zIndex)
        {
            int indexInsideCollection = _sortedElements.IndexOfValue(element);
            if (indexInsideCollection < 0) throw new ArgumentException($"{nameof(T)} is not presented inside collection", nameof(element));

            _sortedElements.RemoveAt(indexInsideCollection);
            _sortedElements.Add(zIndex, element);
        }

        /// <summary>Sets <paramref name="element"/>'s Z-Index to appear at top of collection.</summary>
        /// <param name="element">Element to change Z-Index.</param>
        public void MoveToTop(T element)
        {
            SetZIndex(element, TopIndex + 1);
        }

        /// <summary>Sets <paramref name="element"/>'s Z-Index to appear at the bottom of the collection.</summary>
        /// <param name="element">Element to change Z-Index.</param>
        public void MoveToBottom(T element)
        {
            SetZIndex(element, BottomIndex - 1);
        }

        /// <summary>Swaps the Z-Indexes of <paramref name="firstElement"/> and <paramref name="secondElement"/>.</summary>
        /// <param name="firstElement">First element to swap.</param>
        /// <param name="secondElement">Second element to swap.</param>
        public void Swap(T firstElement, T secondElement)
        {
            int firstElementIndex = _sortedElements.IndexOfValue(firstElement);
            int secondElementIndex = _sortedElements.IndexOfValue(secondElement);

            _sortedElements.SetValueAtIndex(firstElementIndex, secondElement);
            _sortedElements.SetValueAtIndex(secondElementIndex, firstElement);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _sortedElements.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _sortedElements.Values.GetEnumerator();
        }
    }
}
