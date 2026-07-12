using System;
using System.Collections.Generic;
using System.Text;

namespace PopUpUI.PUI
{
    public class PUIDescriptor : IBraceable
    {
        public string TypeName { get; }
        public string ElementName { get; }
        public Dictionary<string, object> Properties { get; }

        public PUIDescriptor(string typeName, string elementName)
        {
            TypeName = typeName;
            ElementName = elementName;
            Properties = new Dictionary<string, object>();
        }

        public void AssignProperty(string name, object value)
        {
            Properties.Add(name, value);
        }
    }
}
