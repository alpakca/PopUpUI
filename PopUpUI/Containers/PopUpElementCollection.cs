using PopUpUI.Elements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

#nullable enable

namespace PopUpUI.Containers
{
    /// <summary>Handler for <see cref="PopUpElementCollection.ChangeOfCollection"/> event.</summary>
    /// <param name="sender">The collection that has modified.</param>
    /// <param name="action">The action presenting modify.</param>
    public delegate void CollectionChangeHandler(PopUpElementCollection sender, CollectionChangeAction action);

    public enum CollectionChangeAction
    {
        Add,
        Remove,
        Refresh
    }

    /// <summary>Presents a container which contains <see cref="PopUpElement"/>s in both ways: regional and singular.</summary>
    [DebuggerDisplay("Count = {Count}")]
    public class PopUpElementCollection : ICollection<PopUpElement>
    {
        internal int _size;
        // object can either be a CollectionRegion or PopUpElement
        private readonly RepeatableSortedList<int, object> _sortedSections;

        bool ICollection<PopUpElement>.IsReadOnly => false;
        /// <summary>Gets total number of <see cref="PopUpElement"/> inside collection.</summary>
        public int Count => _size;
        /// <summary>Gets total number of singular <see cref="PopUpElement"/>s and <see cref="ElementRegion"/>s inside collection.</summary>
        public int CountOfRegionsOrElements => _sortedSections.Count;
        /// <summary>Gets or sets the total number of regions or singulars collection can hold without resizing.</summary>
        public int Capacity
        {
            get => _sortedSections.Capacity;
            set => _sortedSections.Capacity = value;
        }

        /// <summary>Raised when collection has modified.</summary>
        public event CollectionChangeHandler? ChangeOfCollection;

        /// <summary>Initializes a new instance of <see cref="PopUpElementCollection"/> class.</summary>
        public PopUpElementCollection()
        {
            _sortedSections = new RepeatableSortedList<int, object>();
        }

        #region PopUpElement Operations
        /// <summary>Adds an element to collection as singular and places it up most section in collection.</summary>
        /// <param name="element">Element to add.</param>
        public void Add(PopUpElement element)
        {
            _sortedSections.Add(_sortedSections.Max.Key + 1, element);
            _size++;

            FireCollectionChangeEvent(CollectionChangeAction.Add);
        }

        /// <summary>Adds an element to collection as singular with specified <paramref name="zIndex"/>.</summary>
        /// <param name="element">Element to add.</param>
        /// <param name="zIndex">The ZIndex of the element inside collection.</param>
        public void Add(PopUpElement element, int zIndex)
        {
            _sortedSections.Add(zIndex, element);
            _size++;

            FireCollectionChangeEvent(CollectionChangeAction.Add);
        }

        /// <summary>Adds a range of elements to collection as singular with a ZIndex greater than all elements starting from first element of the range.</summary>
        /// <param name="collection">Elements to add to collection.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddRange(IEnumerable<PopUpElement> collection)
        {
            ArgumentNullException.ThrowIfNull(collection);

            foreach (var element in collection) Add(element);
        }

        /// <summary>Adds a range of elements to collection as singular with their ZIndex starting from <paramref name="zIndexStart"/>.</summary>
        /// <param name="collection">Elements to add to collection.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddRange(IEnumerable<PopUpElement> collection, int zIndexStart)
        {
            ArgumentNullException.ThrowIfNull(collection);

            foreach (var element in collection) Add(element, zIndexStart++);
        }

        /// <summary>Removes the given element from collection.</summary>
        /// <param name="element">Element to remove from collection.</param>
        /// <returns><see langword="true"/> if element was successfully removed; otherwise <see langword="false"/>. 
        /// This method also returns <see langword="false"/> if element was not found in the collection.</returns>
        public bool Remove(PopUpElement element)
        {
            int index = _sortedSections.IndexOfValue(element);
            if (index >= 0)
            {
                _sortedSections.RemoveAt(index);
                _size--;

                FireCollectionChangeEvent(CollectionChangeAction.Remove);
            }

            return index >= 0;
        }

        /// <summary>Gets the ZIndex of the <paramref name="element"/>.</summary>
        /// <param name="element">Element to get ZIndex of.</param>
        /// <returns>ZIndex of the element.</returns>
        /// <exception cref="ArgumentException"></exception>
        public int GetZIndex(PopUpElement element)
        {
            return _sortedSections.GetKeyAtIndex(_sortedSections.IndexOfValue(element));
        }

        /// <summary>Sets the ZIndex of the <paramref name="element"/>.</summary>
        /// <param name="element">Element to set ZIndex of.</param>
        /// <param name="zIndex">The new ZIndex of the element.</param>
        /// <exception cref="ArgumentException"></exception>
        public void SetZIndex(PopUpElement element, int zIndex)
        {
            int indexInsideCollection = _sortedSections.IndexOfValue(element);
            if (indexInsideCollection < 0) throw new ArgumentException("PopUpElement is not presented inside PopUpElementCollection", nameof(element));

            _sortedSections.RemoveAt(indexInsideCollection);
            _sortedSections.Add(zIndex, element);
        }
        #endregion

        #region CollectionRegion Operations
        /// <summary>Creates a new <see cref="ElementRegion"/> instance with name <paramref name="name"/> which includes elements from <paramref name="collection"/> 
        /// and places it as up most section in collection.</summary>
        /// <param name="collection">Collection of elements to add to created <see cref="ElementRegion"/>.</param>
        /// <returns>Returns the created <see cref="ElementRegion"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ElementRegion AddAsRegion(IEnumerable<PopUpElement> collection, string? name)
        {
            ArgumentNullException.ThrowIfNull(collection);

            ElementRegion region = new ElementRegion(this) { Name = name ?? string.Empty };
            _sortedSections.Add(_sortedSections.Max.Key + 1, region);

            region.AddRange(collection);

            return region;
        }

        /// <summary>Creates a new <see cref="ElementRegion"/> instance with name <paramref name="name"/> which includes elements from <paramref name="collection"/> 
        /// with specified <paramref name="zIndex"/>.</summary>
        /// <param name="collection">Collection of elements to add to created <see cref="ElementRegion"/>.</param>
        /// <param name="zIndex">The ZIndex of the region inside collection.</param>
        /// <returns>Returns the created <see cref="ElementRegion"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ElementRegion AddAsRegion(IEnumerable<PopUpElement> collection, string? name, int zIndex)
        {
            ArgumentNullException.ThrowIfNull(collection);

            ElementRegion region = new ElementRegion(this) { Name = name ?? string.Empty };
            _sortedSections.Add(zIndex, region);

            region.AddRange(collection);

            return region;
        }

        /// <summary>Removes the <paramref name="region"/> and its elements from collection. 
        /// It also means <paramref name="region"/> is no longer usable.</summary>
        /// <param name="region">Region to remove from collection.</param>
        /// <returns><see langword="true"/> if region was successfully removed; otherwise <see langword="false"/>. 
        /// This method also returns <see langword="false"/> if region was not found in the collection.</returns>
        public bool Remove(ElementRegion region)
        {
            int index = _sortedSections.IndexOfValue(region);
            if (index >= 0)
            {
                _sortedSections.RemoveAt(index);
                region.RemovedFromParent();

                FireCollectionChangeEvent(CollectionChangeAction.Remove);
            }

            return index >= 0;
        }

        /// <summary>Gets the ZIndex of the <paramref name="region"/>.</summary>
        /// <param name="region">Region to get ZIndex of.</param>
        /// <returns>ZIndex of the region.</returns>
        /// <exception cref="ArgumentException"></exception>
        public int GetZIndex(ElementRegion region)
        {
            return _sortedSections.GetKeyAtIndex(_sortedSections.IndexOfValue(region));
        }

        /// <summary>Sets the ZIndex of the <paramref name="region"/>.</summary>
        /// <param name="region">Region to set ZIndex of.</param>
        /// <param name="zIndex">The new ZIndex of the region.</param>
        /// <exception cref="ArgumentException"></exception>
        public void SetZIndex(ElementRegion region, int zIndex)
        {
            int indexInsideCollection = _sortedSections.IndexOfValue(region);
            if (indexInsideCollection < 0) throw new ArgumentException("CollectionRegion is not presented inside PopUpElementCollection", nameof(region));

            _sortedSections.RemoveAt(indexInsideCollection);
            _sortedSections.Add(zIndex, region);
        }
        #endregion

        /// <summary>Clears all of the elements and regions from collection.</summary>
        public void Clear()
        {
            foreach (var section in _sortedSections.Values) if (section is ElementRegion region) region.RemovedFromParent();

            _sortedSections.Clear();
            _size = 0;

            FireCollectionChangeEvent(CollectionChangeAction.Refresh);
        }

        /// <summary>Sets the capacity to the actual number of elements in collection, if that number is less than a threshold value.</summary>
        public void TrimExcess()
        {
            _sortedSections.TrimExcess();
        }

        /// <summary>Determines if <paramref name="item"/> is presented inside collection as singular or inside a region.</summary>
        /// <param name="item">The item to look for.</param>
        /// <returns><see langword="true"/> if <paramref name="item"/> is found in the collection; otherwise <see langword="false"/>.</returns>
        public bool Contains(PopUpElement item)
        {
            foreach (var section in _sortedSections.Values)
            {
                if (section == item || section is ElementRegion region && region.Contains(item)) return true;
            }

            return false;
        }

        /// <summary>Copies the elements of the collection to given array, starting from given index.</summary>
        /// <param name="array">Array to copy elements to.</param>
        /// <param name="arrayIndex">The index to start copying from.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void CopyTo(PopUpElement[] array, int arrayIndex)
        {
            ArgumentNullException.ThrowIfNull(array);

            if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException();

            if (array.Length - arrayIndex < Count) throw new ArgumentException();

            int arrayPasteIndex = arrayIndex;
            foreach (var section in _sortedSections.Values)
            {
                if (section is PopUpElement element)
                {
                    array[arrayPasteIndex] = element;

                    arrayPasteIndex += 1;
                }
                else if (section is ElementRegion region)
                {
                    region.CopyTo(array, arrayPasteIndex);

                    arrayPasteIndex += region.Count;
                }
            }
        }

        #region Item Search
        /// <summary>Searches for the first element that matches the conditions of given predicate.</summary>
        /// <param name="match">Predicate to match with an element.</param>
        /// <returns>If found returns the found element that matches the conditions of given predicate; otherwise <see langword="null"/>.</returns>
        public PopUpElement? Find(Predicate<PopUpElement> match)
        {
            foreach (var element in this)
            {
                if (match(element)) return element;
            }

            return null;
        }

        /// <summary>Searches collection for an element with specified name.</summary>
        /// <param name="name">Name of the element to find.</param>
        /// <returns>If found returns the found element with specified name; otherwise <see langword="null"/>.</returns>
        public PopUpElement? FindByName(string name)
        {
            foreach (var element in this)
            {
                if (element.Name == name) return element;
            }

            return null;
        }

        /// <summary>Searches for the last element that matches the conditions of given predicate.</summary>
        /// <param name="match">Predicate to match with an element.</param>
        /// <returns>If found returns the found element that matches the conditions of given predicate; otherwise <see langword="null"/>.</returns>
        public PopUpElement? FindLast(Predicate<PopUpElement> match)
        {
            using var enumerator = GetReverseEnumerator();

            PopUpElement current;
            while (enumerator.MoveNext())
            {
                current = enumerator.Current;

                if (match(current)) return current;
            }

            return null;
        }

        /// <summary>Searches for the elements that matches the conditions of given predicate starting from end of the collection.</summary>
        /// <param name="match">Predicate to match with elements.</param>
        /// <returns>Elements that matched with conditions of given predicate.</returns>
        public IEnumerable<PopUpElement> WhereLast(Predicate<PopUpElement> match)
        {
            using var enumerator = GetReverseEnumerator();

            PopUpElement current;
            while (enumerator.MoveNext())
            {
                current = enumerator.Current;

                if (match(current)) yield return current;
            }
        }

        internal T FindWithType<T>(Predicate<PopUpElement> match)
        {
            using var enumerator = GetReverseEnumerator();

            PopUpElement current;
            while (enumerator.MoveNext())
            {
                current = enumerator.Current;

                if (current is T tInstance && match(current)) return tInstance;
            }

            return default!;
        }

        internal IEnumerable<T> WhereWithType<T>(Predicate<PopUpElement> match)
        {
            using var enumerator = GetReverseEnumerator();

            PopUpElement current;
            while (enumerator.MoveNext())
            {
                current = enumerator.Current;

                if (current is T tInstance && match(current)) yield return tInstance;
            }
        }
        #endregion

        internal void FireCollectionChangeEvent(CollectionChangeAction action)
        {
            ChangeOfCollection?.Invoke(this, action);
        }

        public IEnumerator<PopUpElement> GetEnumerator()
        {
            foreach (var elementOrRegion in _sortedSections.Values)
            {
                if (elementOrRegion is PopUpElement element)
                {
                    yield return element;
                }
                else if (elementOrRegion is ElementRegion region)
                {
                    foreach (var popUpElement in region) yield return popUpElement;
                }
            }
        }

        private IEnumerator<PopUpElement> GetReverseEnumerator()
        {
            for (int i = CountOfRegionsOrElements - 1; i >= 0; i--)
            {
                if (_sortedSections.Values[i] is PopUpElement element)
                {
                    yield return element;
                }
                else if (_sortedSections.Values[i] is ElementRegion region)
                {
                    for (int j = region.Count - 1; j >= 0; j--) yield return region[j];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
