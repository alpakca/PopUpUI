using Microsoft.Xna.Framework;

namespace PopUpUI.Text
{
    public class SpaceTextPiece : ITextPiece
    {
        private readonly int _count;
        private readonly float _wordSpacing;

        public int VisibleCharLength => _count;

        public SpaceTextPiece(int count, float wordSpacing)
        {
            _count = count;
            _wordSpacing = wordSpacing;
        }

        public float TextWidth()
        {
            return _wordSpacing * _count;
        }

        public float TextWidth(int startIndex)
        {
            return _wordSpacing * (_count - startIndex);
        }

        public float TextWidth(int startIndex, int length)
        {
            return _wordSpacing * length;
        }

        public void Draw(Vector2 position, Color color, float opacity)
        {
        }
    }
}
