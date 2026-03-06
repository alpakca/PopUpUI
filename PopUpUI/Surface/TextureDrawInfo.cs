using Microsoft.Xna.Framework.Graphics;

namespace PopUpUI.Surface
{
    public class TextureDrawInfo : UIDrawInfo
    {
        public Texture2D Texture { get; set; }

        public override Texture2D GetTexture()
        {
            return Texture;
        }
    }
}
