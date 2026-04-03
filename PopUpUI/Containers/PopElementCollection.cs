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

        public PopElement? FindByName(string name)
        {
            for (int i = 0; i < sortedElements.Values.Count; i++)
            {
                if (sortedElements.Values[i].Name == name)
                {
                    return sortedElements.Values[i];
                }
            }

            return null;
        }

        internal IEnumerable<T> WhereWithType<T>(Predicate<T> match)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (sortedElements.Values[i].Visibility != Visibility.Hidden &&
                    sortedElements.Values[i] is T instance && match(instance))
                {
                    yield return instance;
                }
            }
        }

        internal IEnumerable<T> WhereWithType<T>()
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (sortedElements.Values[i].Visibility != Visibility.Hidden &&
                    sortedElements.Values[i] is T instance)
                {
                    yield return instance;
                }
            }
        }

        internal T? FindWithType<T>(Predicate<T> match)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (sortedElements.Values[i].Visibility != Visibility.Hidden && 
                    sortedElements.Values[i] is T instance && match(instance))
                {
                    return instance;
                }
            }

            return default;
        }
    }
}
