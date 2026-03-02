using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PopUpUI.Surface
{
    /// <summary>Stores display information about a texture.</summary>
    public interface IUIDisplayInfo
    {
        /// <summary>Gets or sets the opacity used while rendering texture.</summary>
        public float Opacity { get; set; }
        /// <summary>Gets or sets the size texture will be rendered.</summary>
        public Point Size { get; set; }
        /// <summary>Gets or sets the location of texture on the surface texture will be rendered.</summary>
        public Point Location { get; set; }
        /// <summary>Gets or sets the boundaries will be used while rendering texture.</summary>
        public Rectangle Boundaries { get; set; }

        /// <summary>Gets the texture that will be rendered.</summary>
        /// <returns>Returns the texture to render.</returns>
        public Texture2D GetTexture();
    }
}
