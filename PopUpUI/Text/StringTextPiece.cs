using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;

namespace PopUpUI.Text
{
    /// <summary>Represents a regular string piece with a font associated with it.</summary>
    public class StringTextPiece : ITextPiece
    {
        private readonly string text;
        private readonly SpriteFont font;

        public int VisibleCharLength => text.Length;

        /// <summary>Initializes a new instance of <see cref="StringTextPiece"/> class 
        /// with given <paramref name="text"/> and <paramref name="font"/>.</summary>
        /// <param name="text">The text of the piece.</param>
        /// <param name="font">The font that is associated with given <paramref name="text"/>.</param>
        public StringTextPiece(string text, SpriteFont font)
        {
            this.text = text;
            this.font = font;
        }

        public float TextWidth()
        {
            return font.MeasureString(text).X;
        }

        public float TextWidth(int startIndex)
        {
            return font.MeasureString(text.Substring(startIndex)).X;
        }

        public float TextWidth(int startIndex, int length)
        {
            return font.MeasureString(text.Substring(startIndex, length)).X;
        }

        public void Draw(Vector2 position, Color color, float opacity)
        {
            DrawHelper.Batch.DrawString(font, text, position, color * opacity);
        }
    }
}
