using System;
using System.Collections.Generic;
using System.Text;

namespace PopUpUI.PUI
{
    public abstract class PUIVariable
    {
        public abstract object Value { get; set; }
    }

    public abstract class PUIBraceVariable : PUIVariable, IBraceable
    {
        public abstract void AssignProperty(string name, object value);
    }

    public interface IPUIVariableCreator
    {
        public string TypeName { get; }
        public PUIVariable Get();
    }
}
