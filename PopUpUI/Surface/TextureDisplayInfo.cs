using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PopUpUI.Surface
{
    /// <summary>Stores display information about a <see cref="Texture2D"/>.</summary>
    public class TextureDisplayInfo : IUIDisplayInfo
    {
        private Point _size;

        public float Opacity { get; set; } = 1f;
        public Point Location { get; set; }

        public Point Size
        {
            get => new Point(_size.X != 0 ? _size.X : Texture.Width, _size.Y != 0 ? _size.Y : Texture.Height);
            set => _size = value;
        }

        public Rectangle Boundaries
        {
            get => new Rectangle(Location, Size);
            set
            {
                Location = value.Location;
                Size = value.Size;
            }
        }

        /// <summary>Gets or sets the texture that will be rendered.</summary>
        public Texture2D Texture { get; set; }

        public Texture2D GetTexture()
        {
            return Texture;
        }
    }
}
