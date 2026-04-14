using Microsoft.Xna.Framework;

namespace PopUpUI.Text
{
    /// <summary>Represents a text of a space or spaces.</summary>
    public class SpaceTextPiece : ITextPiece
    {
        private readonly int _count;
        private readonly float _wordSpacing;

        public int VisibleCharLength => _count;

        /// <summary>Initializes a new instance of <see cref="SpaceTextPiece"/> class for a number of spaces and the width of each space.</summary>
        /// <param name="count">Number of spaces this piece contains.</param>
        /// <param name="wordSpacing">Width of each space.</param>
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
