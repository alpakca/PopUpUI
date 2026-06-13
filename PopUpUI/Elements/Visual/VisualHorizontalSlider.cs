using Microsoft.Xna.Framework;
using PopUpUI.Input;
using PopUpUI.Surface;
using System.Collections.Generic;

namespace PopUpUI.Elements.Visual
{
    public class VisualHorizontalSlider : HorizontalSlider
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
                drawInfo.Animation ??= value;

                defaultAnimation = value;
            }
        }

        public Animation2D HoldAnimation { get; set; }

        public VisualHorizontalSlider()
        {
            drawInfo = new AnimationDrawInfo();
        }

        public override void OnHoldStateChange(Input.PopWindowMouseEventArgs e, bool startedHolding)
        {
            drawInfo.Animation = startedHolding && HoldAnimation != null ? HoldAnimation : defaultAnimation;

            base.OnHoldStateChange(e, startedHolding);
        }

        protected override void OnSliding()
        {
            drawInfo.Location = Boundaries.Location + new Point(ScrollOffset, 0);

            base.OnSliding();
        }

        public override IEnumerable<UIDrawInfo> GetDrawInfos()
        {
            return [drawInfo];
        }
    }
}
