using Microsoft.Xna.Framework;

namespace PopUpUI.Text.Rich
{
    public class SingleHashedTextPiece : ITextPiece
    {
        public int VisibleCharLength => RendererHash.VisibleCharLength(Text);

        public string Text { get; set; }
        public IRendererHash RendererHash { get; set; }

        public SingleHashedTextPiece(string text, IRendererHash hash)
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
