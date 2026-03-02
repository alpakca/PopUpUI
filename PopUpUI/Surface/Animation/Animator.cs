using PopUpUI.Toolkit;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace PopUpUI.Surface.Animation
{
    /// <summary>Represents an animator, which controls transitions to various <see cref="AnimationState"/> through parameters and triggers.</summary>
    public class Animator : IEnumerable<AnimationState>
    {
        private float _remainingExitTimeForTransition;
        private AnimationState _pendingState;
        private AnimationState _playingState;

        private readonly List<Transition> _anyStateTransitions;
        private readonly ImmutableDictionary<AnimationState, List<Transition>> _nodes;
        private readonly Dictionary<string, ITransitionParameter> _parameters;

        /// <summary>Gets the state that is currently being played.</summary>
        public AnimationState PlayingState => _playingState;
        /// <summary>Raised when state that is playing has changed.</summary>
        public event Action<Animator, AnimationState> PlayingStateChanged;

        /// <summary>Gets a readonly collection of <see cref="Transition"/> of <paramref name="state"/>.</summary>
        /// <param name="state">The <see cref="AnimationState"/> to get transitions.</param>
        /// <returns>A readonly collection of all <see cref="Transition"/>s <paramref name="state"/> has.</returns>
        public ReadOnlyCollection<Transition> this[AnimationState state] => new ReadOnlyCollection<Transition>(_nodes[state]);

        /// <summary>
        /// Initializes a new instance of <see cref="Animator"/> class with given <paramref name="states"/>, and starts animating with the first element of the collection.
        /// </summary>
        /// <param name="states">The states of the animator.</param>
        public Animator(IEnumerable<AnimationState> states)
        {
            ArgumentNullException.ThrowIfNull(states);

            _nodes = states.ToImmutableDictionary(key => key, state => new List<Transition>());
            _parameters = new Dictionary<string, ITransitionParameter>();

            _playingState = states.First();
            _anyStateTransitions = new List<Transition>();

            PopUpUIManager.RegisterAnimator(this);
        }

        /// <summary>Introduces a new parameter of type <see cref="int"/> to animator to use in a <see cref="Transition"/>.</summary>
        /// <param name="name">The name of the parameter. Must be unique for any other parameters this animator has.</param>
        /// <param name="value">The start value of the parameter.</param>
        public void IntroduceIntParameter(string name, int value)
        {
            _parameters.Add(name, new ValueTransitionParameter<int>(value));
        }

        /// <summary>Introduces a new parameter of type <see cref="float"/> to animator to use in a <see cref="Transition"/>.</summary>
        /// <param name="name">The name of the parameter. Must be unique for any other parameters this animator has.</param>
        /// <param name="value">The start value of the parameter.</param>
        public void IntroduceFloatParameter(string name, float value)
        {
            _parameters.Add(name, new ValueTransitionParameter<float>(value));
        }

        /// <summary>Introduces a new parameter of type <see cref="bool"/> to animator to use in a <see cref="Transition"/>.</summary>
        /// <param name="name">The name of the parameter. Must be unique for any other parameters this animator has.</param>
        /// <param name="value">The start value of the parameter.</param>
        public void IntroduceBoolParameter(string name, bool value)
        {
            _parameters.Add(name, new ValueTransitionParameter<bool>(value));
        }

        /// <summary>Introduces a new parameter of trigger type to animator to use in a <see cref="Transition"/>.</summary>
        /// <param name="name">The name of the parameter. Must be unique for any other parameters this animator has.</param>
        public void IntroduceTriggerParameter(string name)
        {
            _parameters.Add(name, new TriggerTransitionParameter());
        }

        /// <summary>Creates a new transition between states which starts at <paramref name="from"/> and targets <paramref name="to"/>.</summary>
        /// <param name="from">The start state of the transition.</param>
        /// <param name="to">The target state of the transition.</param>
        /// <returns>The created transition between states.</returns>
        public Transition CreateTransition(AnimationState from, AnimationState to)
        {
            Transition transition = new Transition(this, to);

            _nodes[from].Add(transition);
            return transition;
        }

        public Transition CreateTransitionFromAnyState(AnimationState target)
        {
            Transition transition = new Transition(this, target);

            _anyStateTransitions.Add(transition);
            return transition;
        }

        /// <summary>
        /// Searches for a parameter with name <paramref name="parameterName"/> and sets the <paramref name="value"/> to it. <br/>
        /// If any condition dependent on the parameter is satisfied with the new value, the first added transition that meets the condition occurs.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to set <paramref name="value"/>.</param>
        /// <param name="value">The new value of the parameter.</param>
        public void SetInt(string parameterName, int value)
        {
            SetValueToParameter(parameterName, value);
        }

        /// <summary>
        /// Searches for a parameter with name <paramref name="parameterName"/> and sets the <paramref name="value"/> to it. <br/>
        /// If any condition dependent on the parameter is satisfied with the new value, the first added transition that meets the condition occurs.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to set <paramref name="value"/>.</param>
        /// <param name="value">The new value of the parameter.</param>
        public void SetFloat(string parameterName, float value)
        {
            SetValueToParameter(parameterName, value);
        }

        /// <summary>
        /// Searches for a parameter with name <paramref name="parameterName"/> and sets the <paramref name="value"/> to it. <br/>
        /// If any condition dependent on the parameter is satisfied with the new value, the first added transition that meets the condition occurs.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to set <paramref name="value"/>.</param>
        /// <param name="value">The new value of the parameter.</param>
        public void SetBool(string parameterName, bool value)
        {
            SetValueToParameter(parameterName, value);
        }

        /// <summary>
        /// Searches for a parameter with name <paramref name="parameterName"/> and triggers the parameter. <br/>
        /// If there is any, the first added transition with <paramref name="parameterName"/> trigger occurs.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to trigger.</param>
        public void SetTrigger(string parameterName)
        {
            SetValueToParameter(parameterName, new object());
        }

        private void SetValueToParameter<T>(string parameterName, T value)
        {
            foreach (var transition in _nodes[_playingState])
            {
                if (transition.CanTransition(parameterName, value))
                {
                    _parameters[parameterName].SetValue(value);
                    SetPlayingState(transition.Target);

                    return;
                }
            }

            foreach (var transition in _anyStateTransitions)
            {
                if (transition.CanTransition(parameterName, value))
                {
                    _parameters[parameterName].SetValue(value);
                    SetPlayingState(transition.Target);

                    return;
                }
            }
        }

        /// <summary>Gets the value of the parameter named <paramref name="parameterName"/>.</summary>
        /// <param name="parameterName">The name of the parameter to get value.</param>
        /// <returns>The current value of the parameter named <paramref name="parameterName"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        public int GetInt(string parameterName)
        {
            return GetValueFromParameter<int>(parameterName);
        }

        /// <summary>Gets the value of the parameter named <paramref name="parameterName"/>.</summary>
        /// <param name="parameterName">The name of the parameter to get value.</param>
        /// <returns>The current value of the parameter named <paramref name="parameterName"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        public float GetFloat(string parameterName)
        {
            return GetValueFromParameter<float>(parameterName);
        }

        /// <summary>Gets the value of the parameter named <paramref name="parameterName"/>.</summary>
        /// <param name="parameterName">The name of the parameter to get value.</param>
        /// <returns>The current value of the parameter named <paramref name="parameterName"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        public bool GetBool(string parameterName)
        {
            return GetValueFromParameter<bool>(parameterName);
        }

        private T GetValueFromParameter<T>(string parameterName)
        {
            if (_parameters.TryGetValue(parameterName, out ITransitionParameter parameter))
            {
                return parameter.GetValue<T>();
            }

            throw new ArgumentException("parameter not found", parameterName);
        }

#nullable enable
        public AnimationState? FindByName(string name)
        {
            return _nodes.Keys.FirstOrDefault(state => state.Name == name);
        }
#nullable disable

        public IEnumerator<AnimationState> GetEnumerator()
        {
            return _nodes.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal bool TryGetParameter(string parameterName, out ITransitionParameter parameter)
        {
            if (_parameters.TryGetValue(parameterName, out parameter))
            {
                return true;
            }

            parameter = null;
            return false;
        }

        internal void Update()
        {
            // checking if there is a pending state for exit time
            if (_pendingState != null)
            {
                _remainingExitTimeForTransition -= Time.DeltaTime * _playingState.Speed * 1000;
                
                // if exit time has been passed, assign the pending state as playing state
                if (_remainingExitTimeForTransition <= 0)
                {
                    _playingState = _pendingState;
                    _pendingState = null;

                    PlayingStateChanged?.Invoke(this, _playingState);
                } 
            }

            _playingState.Update();
        }

        private void SetPlayingState(AnimationState state)
        {
            // does not accept any transition if there is already a pending animation state
            if (_pendingState != null) return;

            // if playing state has a exit time, we will store new state it as pending and updates its exit time every frame until it is less than zero
            if (_playingState.HasExitTime)
            {
                _remainingExitTimeForTransition = _playingState.ExitTime;
                _pendingState = state;

                return;
            }

            // if playing state doesn't have a exit time, we can assign new state right away
            _playingState = state;
            PlayingStateChanged?.Invoke(this, state);
        }
    }
}
