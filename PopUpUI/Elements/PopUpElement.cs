using Microsoft.Xna.Framework;
using PopUpUI.Input;
using PopUpUI.Surface;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PopUpUI.Elements
{
    /// <summary>Provides as a base class for <see cref="PopUpWindow"/> elements. This is an abstract class.</summary>
    [DebuggerDisplay("{GetType().Name,nq}{Name.Length > 0 ? \", Name: \" + Name : System.String.Empty,nq}")]
    public abstract class PopUpElement
    {
        /// <summary>An empty delegate for <see cref="PopUpElementHandler"/>.</summary>
        public static PopUpElementHandler EmptyHandler { get; } = delegate { };
        /// <summary>An empty delegate that returns <see langword="false"/> for element interfaces, for such cases like <see cref="IHoldable.DoNotHoldIf"/>.</summary>
        public static Predicate<PopUpElement> DoNotActionIfPredicate { get; } = _ => false;

        private bool isFrozen;

        /// <summary>Indicates if element is currently frozen.</summary>
        public bool IsFrozen => isFrozen;
        /// <summary>Gets or sets a name for the element, which can be used to search elements in <see cref="PopUpWindow.Elements"/>.</summary>
        public string Name { get; set; } = "";
        /// <summary>Gets or sets visibility of the element.</summary>
        public Visibility Visibility { get; set; } = Visibility.Visible;
        /// <summary>Gets or sets the options to be used for freezing the element. Default value is <see cref="FreezingOptions.StopAnimationsAndUpdates"/>.</summary>
        public FreezingOptions FreezingOption { get; set; } = FreezingOptions.StopAnimationsAndUpdates;

        /// <summary>
        /// Gets or sets the opacity of the element which affects opacities of <see cref="IUIDisplayInfo"/>s element contains. 
        /// Expected value is between 0.0 and 1.0. Default is 1f.
        /// </summary>
        /// <remarks>When overriden is derived classes, opacity should be set; otherwise it will be zero by default.</remarks>
        public abstract float Opacity { get; set; }
        /// <summary>
        /// Gets or sets the area of the element which affects boundaries of <see cref="IUIDisplayInfo"/>s element contains.
        /// </summary>
        public abstract Rectangle Area { get; set; }

        /// <summary>Update method for the element.</summary>
        public virtual void UpdateElements() { }
        /// <summary>Gets <see cref="IUIDisplayInfo"/>(s) of the element.</summary>
        /// <returns>Returns <see cref="IUIDisplayInfo"/>(s) of the element.</returns>
        public abstract IEnumerable<IUIDisplayInfo> GetDisplayInfos();

        protected abstract void ToggleAnimationStates(bool willPlay);

        public PopUpElement()
        {
            if (this is IHoverable hoverable)
            {
                hoverable.DoNotHoverIf = DoNotActionIfPredicate;
                hoverable.Hovering += EmptyHandler;
                hoverable.HoverStateChange += delegate { };
            }
            if (this is IHoldable holdable)
            {
                holdable.DoNotHoldIf = DoNotActionIfPredicate;
                holdable.Held += EmptyHandler;
                holdable.HoldStateChange += delegate { };
                holdable.ButtonsToHold = MouseButtons.Left;
            }
            if (this is ISelectable selectable)
            {
                selectable.DoNotSelectIf = DoNotActionIfPredicate;
                selectable.Selected += EmptyHandler;
                selectable.OutsideSelected += EmptyHandler;
                selectable.ButtonsToSelect = MouseButtons.Left;
            }
            if (this is IKeyInputReceiver keyReceiver)
            {
                keyReceiver.FinishKeyInputModeIf = DoNotActionIfPredicate;
                keyReceiver.KeyInputReceived += delegate { };
            }
            if (this is IMouseWheelReceiver mouseReceiver)
            {
                mouseReceiver.DoNotReceiveWheelChangeIf = DoNotActionIfPredicate;
                mouseReceiver.MouseWheelChanged += EmptyHandler;
            }
            if (this is ITextInputReceiver textReceiver)
            {
                textReceiver.FinishTextInputModeIf = DoNotActionIfPredicate;
                textReceiver.TextInputReceived += delegate { };
            }
        }

        /// <summary>Checks if given point is inside <see cref="Area"/>.</summary>
        /// <param name="point">Given point to check.</param>
        /// <returns>Returns <see langword="true"/> if given point is inside <see cref="Area"/>, else <see langword="false"/>.</returns>
        public virtual bool IsInside(Point point)
        {
            return Area.Contains(point);
        }

        /// <summary>Freezes the element with given freezing option.
        /// Uses <see cref="FreezingOption"/> if <paramref name="option"/> equals to <see cref="FreezingOptions.CurrentOption"/>.</summary>
        /// <param name="option">The option presenting freeze actions that will be applied.</param>
        public void Freeze(FreezingOptions option = FreezingOptions.CurrentOption)
        {
            isFrozen = true;

            if (option != FreezingOptions.CurrentOption) FreezingOption = option;

            if (FreezingOption.HasFlag(FreezingOptions.StopAnimationStates))
            {
                ToggleAnimationStates(willPlay: false);
            }
        }

        /// <summary>Unfreezes the element.</summary>
        public void Unfreeze()
        {
            isFrozen = false;

            ToggleAnimationStates(willPlay: true);
        }
    }
}
