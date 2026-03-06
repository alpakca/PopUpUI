using Microsoft.Xna.Framework;
using PopUpUI.Elements;
using PopUpUI.Surface;
using System.Collections.Generic;

namespace PopUpUI.NewElements.Visual
{
    internal class VisualMovable : Movable
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

        public VisualMovable(MovementDirection movementDirection) : base(movementDirection)
        {
        }

        public override IEnumerable<UIDrawInfo> GetDrawInfos()
        {
            yield return drawInfo;
        }

        protected override void OnPositionChange()
        {
            drawInfo.Location = _boundaries.Location;
        }
    }
}
