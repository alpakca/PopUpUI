using Microsoft.Xna.Framework;
using PopUpUI.Surface;
using System.Collections.Generic;

namespace PopUpUI.Elements.Visual
{
    internal class VisualCheckBox : CheckBox
    {
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

        public Animation2D Animation
        {
            get => drawInfo.Animation;
            set => drawInfo.Animation = value;
        }

        public VisualCheckBox()
        {
            drawInfo = new AnimationDrawInfo();
        }

        public override IEnumerable<UIDrawInfo> GetDrawInfos()
        {
            return [drawInfo];
        }
    }
}
