using System;
using System.Collections.Generic;
using System.Text;

namespace PopUpUI.Elements
{
    public class ActionCommander<T> : ICommander<T>
    {
        private readonly Action<T> _action;

        public ActionCommander(Action<T> action)
        {
            _action = action;
        }

        public void Execute(T element)
        {
            _action(element);
        }
    }
}
