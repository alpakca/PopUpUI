using System;
using System.Collections.Generic;
using System.Text;

namespace PopUpUI.Elements
{
    public interface ICommander<in TElement>
    {
        public void Execute(TElement element);
    }
}
