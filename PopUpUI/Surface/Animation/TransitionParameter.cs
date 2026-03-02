namespace PopUpUI.Surface.Animation
{
    internal class ValueTransitionParameter<T> : ITransitionParameter
    {
        private T _value;

        public ValueTransitionParameter(T startValue)
        {
            _value = startValue;
        }

        // no boxing / unboxing occurs since TType and T always will be the same type
        public TType GetValue<TType>()
        {
            if (_value is TType tValue) return tValue;

            return default;
        }

        // no boxing / unboxing occurs since TType and T always will be same type
        public void SetValue<TType>(TType value)
        {
            if (value is T tValue) _value = tValue;
        }
    }

    internal class TriggerTransitionParameter : ITransitionParameter
    {
        public TriggerTransitionParameter()
        {
        }

        public T GetValue<T>()
        {
            return default;
        }

        public void SetValue<T>(T value)
        {
        }
    }

    internal interface ITransitionParameter
    {
        public T GetValue<T>();
        public void SetValue<T>(T value);
    }
}
