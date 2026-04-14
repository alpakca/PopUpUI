using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;

namespace PopUpUI.Text.Rich
{
    /// <summary>Represents a hash that is an icon.</summary>
    public class IconHash : IProcesserHash
    {
        public bool WrapsText => false;
        public string Symbol { get; }

        /// <summary>Gets or sets the spacing after icon.</summary>
        public int RightPadding { get; set; }
        /// <summary>Gets or sets an offset to be applied to icon in render.</summary>
        public Point Offset { get; set; }
        /// <summary>Gets or sets the size of icon to be rendered.</summary>
        public Point Size { get; set; }
        /// <summary>Gets or sets the icon texture of the hash.</summary>
        public Texture2D Icon { get; set; }

        /// <summary>Initializes a new instance of <see cref="IconHash"/> class with given symbol and icon.</summary>
        /// <param name="symbol">The symbol representing the icon in text.</param>
        /// <param name="icon">The texture of the icon.</param>
        public IconHash(string symbol, Texture2D icon)
        {
            Symbol = symbol;
            Icon = icon;
        }

        public void Draw(string text, Vector2 position, float opacity)
        {
            DrawHelper.Batch.Draw(Icon, new Rectangle(position.ToPoint() + Offset, Size), Color.White * opacity);
        }

        public float GetWidth(string str)
        {
            if (str.Length == 0)
            {
                return 0;
            }

            return Offset.X + Size.X + RightPadding;
        }

        public int VisibleCharLength(string str)
        {
            // icons are considered to be one-char length
            return 1;
        }
    }
}
