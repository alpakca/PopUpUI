using Microsoft.Xna.Framework;

namespace PopUpUI.Text.Rich
{
    /// <summary>Represents a piece of text with a single <see cref="IProcesserHash"/> associated with.</summary>
    public class SingleHashedTextPiece : ITextPiece
    {
        public int VisibleCharLength => RendererHash.VisibleCharLength(Text);

        public string Text { get; set; }
        public IProcesserHash RendererHash { get; set; }

        /// <summary>Initializes a new instance of <see cref="SingleHashedTextPiece"/> class with given text and hash. </summary>
        /// <param name="text">The text of the piece.</param>
        /// <param name="hash">The hash to be applied on text piece.</param>
        public SingleHashedTextPiece(string text, IProcesserHash hash)
        {
            Text = text;
            RendererHash = hash;
        }

        public float TextWidth()
        {
            return RendererHash.GetWidth(Text);
        }

        public float TextWidth(int startIndex)
        {
            return RendererHash.GetWidth(Text.Substring(startIndex));
        }

        public float TextWidth(int startIndex, int length)
        {
            return RendererHash.GetWidth(Text.Substring(startIndex, length));
        }

        public void Draw(Vector2 position, Color color, float opacity)
        {
            RendererHash.Draw(Text, position, opacity);
        }
    }
}
