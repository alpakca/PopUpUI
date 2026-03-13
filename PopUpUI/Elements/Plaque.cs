using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;
using System;
using System.Collections.Generic;

namespace PopUpUI.Elements
{
    public class Plaque : PopElement
    {
        private readonly UIDrawInfo drawInfo;

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

        public Plaque(bool holdsAnimation)
        {
            drawInfo = holdsAnimation ? new AnimationDrawInfo() : new TextureDrawInfo();
        }

        public Plaque(Texture2D texture)
        {
            drawInfo = new TextureDrawInfo { Texture = texture };
        }

        public Plaque(Animation2D animation)
        {
            drawInfo = new AnimationDrawInfo { Animation = animation };
        }

        public void SetNew(Texture2D texture)
        {
            if (drawInfo is not TextureDrawInfo)
            {
                throw new ArgumentException("plaque holds animation; texture was given", nameof(texture));
            }

            ((TextureDrawInfo)drawInfo).Texture = texture;
        }

        public void SetNew(Animation2D animation)
        {
            if (drawInfo is not AnimationDrawInfo)
            {
                throw new ArgumentException("plaque holds texture; animation was given", nameof(animation));
            }

            ((AnimationDrawInfo)drawInfo).Animation = animation;
        }

        public override IEnumerable<UIDrawInfo> GetDrawInfos()
        {
            yield return drawInfo;
        }
    }
}
