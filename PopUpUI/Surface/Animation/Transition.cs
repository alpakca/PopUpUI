using System;
using System.Collections.Generic;

namespace PopUpUI.Surface.Animation
{
    /// <summary>Represents the transition between one <see cref="AnimationState"/> to another, which results in change of <see cref="Animator.PlayingState"/>.</summary>
    public class Transition
    {
        private readonly Animator _animator;
        private readonly AnimationState _transitionTo;
        private readonly Dictionary<ITransitionParameter, ITransitionCondition> ConditionOfParameters;

        /// <summary>The state this transition targets to.</summary>
        public AnimationState Target => _transitionTo;

        internal Transition(Animator animator, AnimationState transitionTo)
        {
            _animator = animator;
            _transitionTo = transitionTo;
            ConditionOfParameters = new Dictionary<ITransitionParameter, ITransitionCondition>();
        }

        /// <summary>Adds the <paramref name="condition"/> to the parameter named <paramref name="parameterName"/> 
        /// which was registered to the <see cref="Animator"/> this instance was created.</summary>
        /// <param name="parameterName">The name of the parameter to add the condition.</param>
        /// <param name="condition">The condition to add.</param>
        /// <returns>A reference to this instance after the condition has been added.</returns>
        public Transition AddIntCondition(string parameterName, Func<int, bool> condition)
        {
            if (_animator.TryGetParameter(parameterName, out ITransitionParameter foundParameter))
            {
                ConditionOfParameters[foundParameter] = new ValueTransitionCondition<int>(condition);
            }

            return this;
        }

        /// <summary>Adds the <paramref name="condition"/> to the parameter named <paramref name="parameterName"/> 
        /// which was registered to the <see cref="Animator"/> this instance was created.</summary>
        /// <param name="parameterName">The name of the parameter to add the condition.</param>
        /// <param name="condition">The condition to add.</param>
        /// <returns>A reference to this instance after the condition has been added.</returns>
        public Transition AddFloatCondition(string parameterName, Func<float, bool> condition)
        {
            if (_animator.TryGetParameter(parameterName, out ITransitionParameter foundParameter))
            {
                ConditionOfParameters[foundParameter] = new ValueTransitionCondition<float>(condition);
            }

            return this;
        }

        /// <summary>Adds the <paramref name="condition"/> to the parameter named <paramref name="parameterName"/> 
        /// which was registered to the <see cref="Animator"/> this instance was created.</summary>
        /// <param name="parameterName">The name of the parameter to add the condition.</param>
        /// <param name="condition">The condition to add.</param>
        /// <returns>A reference to this instance after the condition has been added.</returns>
        public Transition AddBoolCondition(string parameterName, Func<bool, bool> condition)
        {
            if (_animator.TryGetParameter(parameterName, out ITransitionParameter foundParameter))
            {
                ConditionOfParameters[foundParameter] = new ValueTransitionCondition<bool>(condition);
            }

            return this;
        }

        /// <summary>Adds a trigger condition to the parameter named <paramref name="parameterName"/> 
        /// which was registered to the <see cref="Animator"/> this instance was created.</summary>
        /// <param name="parameterName">The name of the parameter to add the condition.</param>
        /// <returns>A reference to this instance after the condition has been added.</returns>
        public Transition AddTriggerCondition(string parameterName)
        {
            if (_animator.TryGetParameter(parameterName, out ITransitionParameter foundParameter))
            {
                ConditionOfParameters[foundParameter] = new TriggerTransitionCondition();
            }

            return this;
        }

        internal bool CanTransition<T>(string parameterName, T value)
        {
            if (_animator.TryGetParameter(parameterName, out ITransitionParameter foundParameter))
            {
                if (ConditionOfParameters.TryGetValue(foundParameter, out ITransitionCondition foundCondition))
                {
                    return foundCondition.PassesCondition(value);
                }
            }

            return false;
        }
    }
}
