using System;
using System.Collections.Generic;
using System.Text;

namespace PopUpUI.PUI
{
    public interface IDescriptorAssigner<T>
    {
        public string TypeName { get; }
        public void Assign(PUIDescriptor desc, T element);
    }
}
