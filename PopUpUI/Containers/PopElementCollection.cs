using PopUpUI.Elements;
using System;
using System.Collections.Generic;

#nullable enable

namespace PopUpUI.Containers
{
    public class PopElementCollection : ZIndexCollection<PopElement>
    {
        public PopElementCollection() : base(Comparer<int>.Default)
        {
        }

        /// <summary>Searches for an element in collection with given <paramref name="name"/> starting from top element.</summary>
        /// <param name="name">The name of the <see cref="PopElement"/>.</param>
        /// <returns>The first <see cref="PopElement"/> that's name is same as <paramref name="name"/>, 
        /// or <see langword="null"/> if no elements found with <paramref name="name"/>.</returns>
        public PopElement? FindByName(string name)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (SortedElements.Values[i].Name == name)
                {
                    return SortedElements.Values[i];
                }
            }

            return null;
        }

        internal IEnumerable<T> WhereWithType<T>(Predicate<T> match)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (SortedElements.Values[i].Visibility != Visibility.Hidden &&
                    SortedElements.Values[i] is T instance && match(instance))
                {
                    yield return instance;
                }
            }
        }

        internal IEnumerable<T> WhereWithType<T>()
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (SortedElements.Values[i].Visibility != Visibility.Hidden &&
                    SortedElements.Values[i] is T instance)
                {
                    yield return instance;
                }
            }
        }

        internal T? FindWithType<T>(Predicate<T> match)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (SortedElements.Values[i].Visibility != Visibility.Hidden && 
                    SortedElements.Values[i] is T instance && match(instance))
                {
                    return instance;
                }
            }

            return default;
        }
    }
}
