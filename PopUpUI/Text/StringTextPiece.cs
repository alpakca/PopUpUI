using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;

namespace PopUpUI.Text
{
    public class StringTextPiece : ITextPiece
    {
        private readonly string text;
        private readonly SpriteFont font;

        public int VisibleCharLength => text.Length;

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
