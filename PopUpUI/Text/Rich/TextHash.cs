using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;

namespace PopUpUI.Text.Rich
{
    /// <summary>Represents a hash that can render a regular text with different color and font.</summary>
    public class TextHash : IProcesserHash
    {
        public bool WrapsText => true;
        public string Symbol { get; }

        /// <summary>The color to be applied on text.</summary>
        public Color Color { get; set; }
        /// <summary>The font to be used for text.</summary>
        public SpriteFont Font { get; set; }

        /// <summary>Initializes a new instance of <see cref="TextHash"/> class with given symbol, color and font.</summary>
        /// <param name="symbol"></param>
        /// <param name="color"></param>
        /// <param name="font"></param>
        public TextHash(string symbol, Color color, SpriteFont font)
        {
            Symbol = symbol;
            Color = color;
            Font = font;
        }

        public void Draw(string text, Vector2 position, float opacity)
        {
            DrawHelper.Batch.DrawString(Font, text, position, Color * opacity);
        }

        public float GetWidth(string str)
        {
            return Font.MeasureString(str).X;
        }

        public int VisibleCharLength(string str)
        {
            return str.Length;
        }
    }
}
