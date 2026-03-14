using Microsoft.Xna.Framework;

namespace PopUpUI.Text
{
    public class HashedTextPiece
    {
        public int VisibleCharLength => RendererHash.VisibleCharLength(Text);

        public string Text { get; set; }
        public IRendererHash RendererHash { get; set; }

        public HashedTextPiece(string text, IRendererHash hash)
        {
            Text = text;
            RendererHash = hash;
        }

        public float TextWidth()
        {
            return RendererHash.GetWidth(Text);
        }

        public float TextWidth(int startIndex, int length)
        {
            return RendererHash.GetWidth(Text.Substring(startIndex, length));
        }

        public void DrawHash(Vector2 position)
        {
            RendererHash.Draw(Text, position);
        }
    }
}
