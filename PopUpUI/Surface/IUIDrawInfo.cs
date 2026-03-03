using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PopUpUI.Surface
{
    public interface IUIDrawInfo
    {
        public float Opacity { get; }
        public Rectangle DestinationArea { get; }
        public Texture2D Texture { get; }
    }
}
