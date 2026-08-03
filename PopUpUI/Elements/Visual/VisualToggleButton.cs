using Microsoft.Xna.Framework;
using PopUpUI.Surface;
using System.Collections.Generic;

namespace PopUpUI.Elements.Visual
{
    public class VisualToggleButton : ToggleButton
    {
        private Animation2D _checkedAnimation;
        private Animation2D _uncheckedAnimation;
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

        public Animation2D CheckedAnimation
        {
            get => _checkedAnimation;
            set
            {
                // special case for initial animating, which should also assign draw info
                // so that it won't return null before any event occurs
                drawInfo.Animation ??= value;
                _checkedAnimation = value;

                if (UncheckedAnimation != null)
                {
                    SetDrawInfoAnimation();
                }
            }
        }

        public Animation2D UncheckedAnimation
        {
            get => _uncheckedAnimation;
            set
            {
                // special case for initial animating, which should also assign draw info
                // so that it won't return null before any event occurs
                drawInfo.Animation ??= value;
                _uncheckedAnimation = value;

                if (CheckedAnimation != null)
                {
                    SetDrawInfoAnimation();
                }
            }
        }

        public Animation2D UnenabledAnimation { get; set; }

        public VisualToggleButton()
        {
            drawInfo = new AnimationDrawInfo();
        }

        protected override void OnToggle()
        {
            SetDrawInfoAnimation();
            base.OnToggle();
        }

        protected override void EnableStatusChanged()
        {
            SetDrawInfoAnimation();
            base.EnableStatusChanged();
        }

        private void SetDrawInfoAnimation()
        {
            if (!this.Enabled)
            {
                drawInfo.Animation = UnenabledAnimation;
                return;
            }

            drawInfo.Animation = IsChecked ? CheckedAnimation : UncheckedAnimation;
        }

        public override IEnumerable<UIDrawInfo> GetDrawInfos()
        {
            return [drawInfo];
        }
    }
}
