using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PopUpUI.Surface
{
    public abstract class UIDrawInfo
    {
        private Rectangle destinationRect;

        public Point Location
        {
            get => destinationRect.Location;
            set => destinationRect.Location = value;
        }

        public Point Size
        {
            get => destinationRect.Size;
            set => destinationRect.Size = value;
        }

        public Rectangle DestinationArea
        {
            get => destinationRect;
            set => destinationRect = value;
        }

        public float Opacity { get; set; } = 1f;

        public abstract Texture2D GetTexture();
    }
}
