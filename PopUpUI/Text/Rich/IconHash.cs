using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;

namespace PopUpUI.Text.Rich
{
    public class IconHash : IRendererHash
    {
        public bool WrapsText => false;
        public string Symbol { get; }

        public int RightPadding { get; set; }
        public Point Offset { get; set; }
        public Point Size { get; set; }
        public Texture2D Icon { get; set; }

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
