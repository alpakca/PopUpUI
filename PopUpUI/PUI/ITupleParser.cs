using PopUpUI.PUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace PopUpUI.PUI
{
    public interface ITupleParser
    {
        public string TypeName { get; }
        public object Parse(object[] values);
    }
}
