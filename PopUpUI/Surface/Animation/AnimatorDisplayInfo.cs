using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PopUpUI.Surface.Animation
{
    /// <summary>Stores display information about a <see cref="AnimationState/>.</summary>
    public class AnimationDisplayInfo : IUIDisplayInfo
    {
        private Point _size;

        public float Opacity { get; set; } = 1f;
        public Point Location { get; set; }

        public Point Size
        {
            get => new Point(_size.X != 0 ? _size.X : Animation.Width, _size.Y != 0 ? _size.Y : Animation.Height);
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

        /// <summary>Gets or sets the animation that will be rendered.</summary>
        public AnimationState Animation { get; set; }

        public Texture2D GetTexture()
        {
            return Animation;
        }
    }
}
