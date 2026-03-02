using PopUpUI.Elements;
using PopUpUI.Input;
using PopUpUI.Surface;
using PopUpUI.Surface.Animation;
using System;

#nullable enable

namespace PopUpUI.Toolkit
{
    internal static class PopUpElementHelper
    {
        public static Animator GetHoldHoverAnimator<T>(AnimationState defaultState, AnimationState hoveredState, AnimationState heldState,
                                                       T element, string hoveredParameterName = "Hovered", string heldParameterName = "Held")
            where T : PopUpElement, IHoverable, IHoldable
        {
            Animator animator = new Animator([defaultState, hoveredState, heldState]);

            animator.IntroduceBoolParameter(hoveredParameterName, element.IsHovered);
            animator.IntroduceBoolParameter(heldParameterName, element.IsHeld);

            element.HoverStateChange += (_, _, isHovered) => animator.SetBool(hoveredParameterName, isHovered);
            element.HoldStateChange += (_, _, isHeld) => animator.SetBool(heldParameterName, isHeld);

            animator.CreateTransition(defaultState, hoveredState).AddBoolCondition(hoveredParameterName, isHovered => isHovered);
            animator.CreateTransition(defaultState, heldState).AddBoolCondition(heldParameterName, isHeld => isHeld);

            animator.CreateTransitionFromAnyState(defaultState).AddBoolCondition(hoveredParameterName, isHovered => !isHovered && !element.IsHeld)
                                                               .AddBoolCondition(heldParameterName, isHeld => !isHeld && !element.IsHovered);

            animator.CreateTransition(hoveredState, heldState).AddBoolCondition(heldParameterName, isHeld => isHeld);
            animator.CreateTransition(heldState, hoveredState).AddBoolCondition(heldParameterName, isHeld => !isHeld && element.IsHovered);

            return animator;
        }

        public static Animator GetHoverAnimator<T>(AnimationState defaultState, AnimationState hoveredState, T element, string hoveredParameterName = "Hovered")
            where T : PopUpElement, IHoverable
        {
            Animator animator = new Animator([defaultState, hoveredState]);

            animator.IntroduceBoolParameter(hoveredParameterName, element.IsHovered);

            element.HoverStateChange += (_, _, isHovered) => animator.SetBool(hoveredParameterName, isHovered);

            animator.CreateTransition(defaultState, hoveredState).AddBoolCondition(hoveredParameterName, isHovered => isHovered);
            animator.CreateTransition(hoveredState, defaultState).AddBoolCondition(hoveredParameterName, isHovered => !isHovered);

            return animator;
        }
    }
}
