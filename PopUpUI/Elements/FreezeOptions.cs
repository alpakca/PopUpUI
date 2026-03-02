using System;

namespace PopUpUI.Elements
{
    /// <summary>Specifies the actions to be taken when a <see cref="PopUpWindow"/> or <see cref="Elements.PopUpElement"/> frozen.</summary>
    [Flags]
    public enum FreezingOptions
    {
        /// <summary>Freezing does nothing.</summary>
        Nothing,
        /// <summary>Freezing pauses <see cref="AnimationState"/>s unfreezing plays the animations where they left off.</summary>
        StopAnimationStates = 1,
        /// <summary>Freezing stops the updates elements or/and windows gets.</summary>
        StopUpdates = 2,
        /// <summary>Freezing pauses animations and stops the updates.</summary>
        StopAnimationsAndUpdates = StopAnimationStates | StopUpdates,
        /// <summary>Used to freeze elements or windows with the option they had, without passing same options again.</summary>
        CurrentOption = 4
    }
}
