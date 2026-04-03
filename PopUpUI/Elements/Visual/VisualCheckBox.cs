using Microsoft.Xna.Framework;
using PopUpUI.Surface;
using System.Collections.Generic;

namespace PopUpUI.Elements.Visual
{
    public class VisualCheckBox : CheckBox
    {
        private Animation2D _checkedAnimation;
        private Animation2D _uncheckedAnimation;
        private readonly AnimationDrawInfo drawInfo;

        public override float Opacity 
        { 
            set
            {
                drawInfo.Opacity = value;
                base.Opacity = value;
            }
        }

        public override Rectangle Boundaries 
        {
            set
            {
                drawInfo.DestinationArea = value;
                base.Boundaries = value;
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

        public VisualCheckBox()
        {
            drawInfo = new AnimationDrawInfo();
        }

        protected override void OnCheckChange()
        {
            drawInfo.Animation = IsChecked ? CheckedAnimation : UncheckedAnimation;

            base.OnCheckChange();
        }

        public override IEnumerable<UIDrawInfo> GetDrawInfos()
        {
            return [drawInfo];
        }
    }
}
