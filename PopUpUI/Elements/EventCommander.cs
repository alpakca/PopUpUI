using System;
using System.Collections.Generic;
using System.Text;

#nullable enable

namespace PopUpUI.Elements
{
    public class EventCommander<T> : ICommander<T>
    {
        public event Action<T>? Commands;

        public void Execute(T element)
        {
            Commands?.Invoke(element);
        }
    }
}
