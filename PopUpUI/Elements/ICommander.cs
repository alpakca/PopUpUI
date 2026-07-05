using System;
using System.Collections.Generic;
using System.Text;

namespace PopUpUI.Elements
{
    public interface ICommander<in T>
    {
        public void Execute(T element);
    }
}
