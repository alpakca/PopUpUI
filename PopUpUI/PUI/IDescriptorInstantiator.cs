using System;
using System.Collections.Generic;
using System.Text;

namespace PopUpUI.PUI
{
    public interface IDescriptorInstantiator<T>
    {
        public string TypeName { get; }
        public T Instantiate(PUIDescriptor desc);
    }
}
