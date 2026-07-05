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
            }
        }

        public VisualToggleButton()
        {
            drawInfo = new AnimationDrawInfo();
        }

        protected override void OnToggle()
        {
            drawInfo.Animation = IsChecked ? CheckedAnimation : UncheckedAnimation;

            base.OnToggle();
        }

        public override IEnumerable<UIDrawInfo> GetDrawInfos()
        {
            return [drawInfo];
        }
    }
}
