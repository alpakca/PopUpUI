using Microsoft.Xna.Framework;
using PopUpUI.Input;
using PopUpUI.Surface;

namespace PopUpUI.NewElements.Visual
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

        public VisualHorizontalSlider(int range) : base(range)
        {
        }

        public override void OnHoldStateChange(PopUpWindowEventArgs e, bool startedHolding)
        {
            drawInfo.Animation = startedHolding && HoldAnimation != null ? HoldAnimation : defaultAnimation;

            base.OnHoldStateChange(e, startedHolding);
        }

        protected override void OnSliding()
        {
            drawInfo.Location = _boundaries.Location + new Point(xOffset, 0);

            base.OnSliding();
        }
    }
}
