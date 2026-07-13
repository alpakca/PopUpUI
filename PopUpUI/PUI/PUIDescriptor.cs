using System;
using System.Collections.Generic;
using System.Text;

namespace PopUpUI.PUI
{
    public class PUIDescriptor : IBraceable
    {
        private readonly Dictionary<string, object> properties;

        public string TypeName { get; }
        public string ElementName { get; }

        public PUIDescriptor(string typeName, string elementName)
        {
            TypeName = typeName;
            ElementName = elementName;
            properties = new Dictionary<string, object>();
        }

        public void AssignProperty(string name, object value)
        {
            properties.Add(name, value);
        }

        public T GetProperty<T>(string name)
        {
            return (T)properties[name];
        }

        public bool TryGetProperty<T>(string name, out T result)
        {
            if (properties.TryGetValue(name, out object oValue) && oValue is T tValue)
            {
                result = tValue;
                return true;
            }

            result = default;
            return false;
        }

        public T GetPropertyOrDefault<T>(string name, T defaultValue)
        {
            if (properties.TryGetValue(name, out object oValue) && oValue is T value)
            {
                return value;
            }

            return defaultValue;
        }
    }
}
