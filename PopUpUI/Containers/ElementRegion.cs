using PopUpUI.Elements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

#nullable enable

namespace PopUpUI.Containers
{
    /// <summary>Presents a collection of <see cref="PopUpElement"/>s.</summary>
    [DebuggerDisplay("Count = {Count}{Name.Length > 0 ? \", Name: \" + Name : System.String.Empty,nq}")]
    public class ElementRegion : ZIndexCollection<PopUpElement>
    {
        private bool removed = false;
        private readonly PopUpElementCollection parent;

        /// <summary>Gets or sets a name for the region.</summary>
        public string Name { get; set; } = "";

        internal ElementRegion(PopUpElementCollection parent)
        {
            this.parent = parent;
        }

        internal void RemovedFromParent()
        {
            removed = true;
            parent._size -= this.Count;
        }

        // We don't want to use these classes to work with indexes; instead elements should be used.
        // But we still use indexing for reverse enumerating in PopUpElementCollection for a cleaner approach
        internal PopUpElement this[int index]
        {
            get => sortedElements.GetValueAtIndex(index);
        }

        /// <summary>Adds an element and places it up most section in region.</summary>
        /// <param name="element">Element to add.</param>
        public override void Add(PopUpElement element)
        {
            base.Add(element);

            if (!removed) parent._size++;
            parent.FireCollectionChangeEvent(CollectionChangeAction.Add);
        }

        /// <summary>Adds an element to region with specified <paramref name="zIndex"/>.</summary>
        /// <param name="element">Element to add.</param>
        /// <param name="zIndex">The ZIndex of the element inside region.</param>
        public override void Add(PopUpElement element, int zIndex)
        {
            base.Add(element, zIndex);

            if (!removed) parent._size++;
            parent.FireCollectionChangeEvent(CollectionChangeAction.Add);
        }

        /// <summary>Adds a range of elements to region with a ZIndex greater than all elements starting from first element of the range.</summary>
        /// <param name="collection">Elements to add to region.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public override void AddRange(IEnumerable<PopUpElement> collection)
        {
            foreach (var element in collection)
            {
                Add(element);
            }
        }

        /// <summary>Adds a range of elements to region with their ZIndex starting from <paramref name="zIndexStart"/>.</summary>
        /// <param name="collection">Elements to add to region.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public override void AddRange(IEnumerable<PopUpElement> collection, int zIndexStart)
        {
            foreach (var element in collection)
            {
                Add(element, zIndexStart++);
            }
        }

        /// <summary>Clears all of the elements inside region.</summary>
        public override void Clear()
        {
            if (!removed) parent._size -= this.Count;

            base.Clear();

            parent.FireCollectionChangeEvent(CollectionChangeAction.Remove);
        }

        /// <summary>Removes the given element from region.</summary>
        /// <param name="element">Element to remove from region.</param>
        /// <returns><see langword="true"/> if element was successfully removed; otherwise <see langword="false"/>. 
        /// This method also returns <see langword="false"/> if element was not found in the region.</returns>
        public override bool Remove(PopUpElement element)
        {
            if (base.Remove(element))
            {
                if (!removed) parent._size--;
                parent.FireCollectionChangeEvent(CollectionChangeAction.Remove);

                return true;
            }

            return false;
        }

        /// <summary>Searches for the first element that matches the conditions of given predicate.</summary>
        /// <param name="match">Predicate to match with an element.</param>
        /// <returns>If found returns the found element that matches the conditions of given predicate; otherwise <see langword="null"/>.</returns>
        public PopUpElement? Find(Predicate<PopUpElement> match)
        {
            return sortedElements.FindValue(match);
        }

        /// <summary>Searches for the last element that matches the conditions of given predicate.</summary>
        /// <param name="match">Predicate to match with an element.</param>
        /// <returns>If found returns the found element that matches the conditions of given predicate; otherwise <see langword="null"/>.</returns>
        public PopUpElement? FindLast(Predicate<PopUpElement> match)
        {
            return sortedElements.FindLastValue(match);
        }
    }
}
