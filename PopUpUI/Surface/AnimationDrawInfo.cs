using Microsoft.Xna.Framework.Graphics;

namespace PopUpUI.Surface
{
    public class AnimationDrawInfo : UIDrawInfo
    {
        public Animation2D Animation { get; set; }

        public override Texture2D GetTexture()
        {
            return Animation.CurrentFrame;
        }
    }
}
