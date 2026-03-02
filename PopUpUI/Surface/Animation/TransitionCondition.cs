using System;

namespace PopUpUI.Surface.Animation
{
    internal class ValueTransitionCondition<T> : ITransitionCondition
    {
        private readonly Func<T, bool> _condition;

        public ValueTransitionCondition(Func<T, bool> condition)
        {
            _condition = condition;
        }

        // no boxing / unboxing occurs since TType and T always will be same type
        public bool PassesCondition<TType>(TType value)
        {
            return value is T tValue && _condition(tValue);
        }
    }

    internal class TriggerTransitionCondition : ITransitionCondition
    {
        public bool PassesCondition<T>(T value)
        {
            return true;
        }
    }

    internal interface ITransitionCondition
    {
        public bool PassesCondition<T>(T value);
    }
}
