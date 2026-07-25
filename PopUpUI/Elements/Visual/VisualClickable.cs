using Microsoft.Xna.Framework;
using PopUpUI.Input;
using PopUpUI.Surface;
using System.Collections.Generic;

namespace PopUpUI.Elements.Visual
{
    public class VisualClickable : Clickable
    {
        private Animation2D defaultAnimation;
        private readonly AnimationDrawInfo drawInfo;

        public override Rectangle Boundaries 
        { 
            set
            {
                drawInfo.DestinationArea = value;
                base.Boundaries = value;
            }
        }

        public override float Opacity 
        {
            set 
            {
                drawInfo.Opacity = value;
                base.Opacity = value;
            } 
        }

        public Animation2D DefaultAnimation
        {
            get => defaultAnimation;
            set
            {
                // special case for initial animating, which should also assign draw info
                // so that it won't return null before any event occurs
                drawInfo.Animation ??= value;

                defaultAnimation = value;
            }
        }

        public Animation2D UnenabledAnimation { get; set; }

        public Animation2D HoldAnimation { get; set; }
        public Animation2D HoverAnimation { get; set; }

        public VisualClickable()
        {
            drawInfo = new AnimationDrawInfo();
        }

        protected override void EnableStatusChanged()
        {
            drawInfo.Animation = GetDrawInfoAnimation(IsHeld, IsHovered);

            base.EnableStatusChanged();
        }

        protected override void OnClick()
        {
            // IsHeld must be false to be able to click
            drawInfo.Animation = GetDrawInfoAnimation(false, IsHovered);

            base.OnClick();
        }

        public override void OnHoldStateChange(PopWindowMouseEventArgs e, bool startedHolding)
        {
            drawInfo.Animation = GetDrawInfoAnimation(startedHolding, IsHovered);

            base.OnHoldStateChange(e, startedHolding);
        }

        public override void OnHoverStateChange(PopWindowMouseEventArgs e, bool startedHovering)
        {
            drawInfo.Animation = GetDrawInfoAnimation(IsHeld, startedHovering);

            base.OnHoverStateChange(e, startedHovering);
        }

        public override IEnumerable<UIDrawInfo> GetDrawInfos()
        {
            return [drawInfo];
        }

        private Animation2D GetDrawInfoAnimation(bool isHeld, bool isHovered)
        {
            if (!Enabled)
            {
                return UnenabledAnimation;
            }

            if (isHeld)
            {
                return HoldAnimation ?? (IsHeld ? HoverAnimation ?? defaultAnimation : defaultAnimation);
            }
            else if (isHovered)
            {
                return HoverAnimation ?? defaultAnimation;
            }

            return defaultAnimation;
        }
    }
}
