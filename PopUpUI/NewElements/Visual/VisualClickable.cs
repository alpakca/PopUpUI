using Microsoft.Xna.Framework;
using PopUpUI.Input;
using PopUpUI.Surface;
using System.Collections.Generic;

namespace PopUpUI.NewElements.Visual
{
    internal class VisualClickable : Clickable
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

        public Animation2D HoldAnimation { get; set; }

        protected override void OnClick()
        {
            drawInfo.Animation = defaultAnimation;

            base.OnClick();
        }

        public override void OnSelect(PopUpWindowEventArgs e)
        {
            drawInfo.Animation = HoldAnimation ?? defaultAnimation;

            base.OnSelect(e);
        }

        public override IEnumerable<UIDrawInfo> GetDrawInfos()
        {
            yield return drawInfo;
        }
    }
}
