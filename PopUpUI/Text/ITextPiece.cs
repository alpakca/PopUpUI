using Microsoft.Xna.Framework;

namespace PopUpUI.Text
{
    public interface ITextPiece
    {
        public int VisibleCharLength { get; }

        public float TextWidth();
        public float TextWidth(int startIndex);
        public float TextWidth(int startIndex, int length);
        public void Draw(Vector2 position, Color color, float opacity);
    }
}
