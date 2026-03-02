using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PopUpUI.Text
{
    internal class HashCollection<T> : IList<T>
    {
        private readonly RichTextRenderer renderer;
        private readonly List<T> collection;

        public int Count => collection.Count;
        public bool IsReadOnly => false;

        public event Action CollectionChanged = delegate { };

        internal HashCollection(RichTextRenderer renderer)
        {
            this.renderer = renderer;
            collection = new List<T>();
        }

        public T this[int index]
        {
            get => collection[index];
            set
            {
                ThrowIfNotProper(value);
                collection[index] = value;

                CollectionChanged();
            }
        }

        public void Add(T item)
        {
            ThrowIfNotProper(item);
            collection.Add(item);

            CollectionChanged();
        }

        public void Clear()
        {
            if (collection.Count != 0)
            {
                collection.Clear();

                CollectionChanged();
            }
        }

        public bool Contains(T item)
        {
            return collection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            collection.CopyTo(array, arrayIndex);
        }

        public int IndexOf(T item)
        {
            return collection.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ThrowIfNotProper(item);

            collection.Insert(index, item);

            CollectionChanged();
        }

        public bool Remove(T item)
        {
            if (collection.Remove(item))
            {
                CollectionChanged();
                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            collection.RemoveAt(index);

            CollectionChanged();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void ThrowIfNotProper(T hash)
        {
            var hashes = renderer.ColorHashes.SelectMany(colorHash => (IEnumerable<string>)[colorHash.StartHash, colorHash.EndHash])
                 .Concat(renderer.IconHashes.Select(iconHash => iconHash.Hash));

            if (hash is ColorHash colorHash)
            {
                if (hashes.Any(hash => hash == colorHash.StartHash || hash == colorHash.EndHash))
                    throw new ArgumentException("Hash must be different from any hash this TextBlock already has.");
            }
            else if (hash is IconHash iconHash)
            {
                if (hashes.Any(hash => hash == iconHash.Hash))
                    throw new ArgumentException("Hash must be different from any hash this TextBlock already has.");
            }
        }
    }
}
