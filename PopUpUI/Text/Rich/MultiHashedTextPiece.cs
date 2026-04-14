using System;
using Microsoft.Xna.Framework;

namespace PopUpUI.Text.Rich
{
    /// <summary>Represents a piece of text that contains multiple hashes.</summary>
    public class MultiHashedTextPiece : ITextPiece
    {
        private readonly int _length;
        private readonly string[] _hashedParts;
        private readonly IProcesserHash[] _hashes;

        public int VisibleCharLength => _length;

        /// <summary>Initializes a new instance of <see cref="MultiHashedTextPiece"/> class with given hash strings and their hashes.</summary>
        /// <param name="hashedParts">Array of strings that have different associated hashes.</param>
        /// <param name="hashes">Array of hashes that are corresponding to <paramref name="hashedParts"/>.</param>
        public MultiHashedTextPiece(string[] hashedParts, IProcesserHash[] hashes)
        {
            _hashedParts = hashedParts;
            _hashes = hashes;

            for (int i = 0; i < hashedParts.Length; i++)
            {
                _length += _hashes[i].VisibleCharLength(_hashedParts[i]);
            }
        }

        public float TextWidth()
        {
            return TextWidth(0, _length);
        }

        public float TextWidth(int startIndex)
        {
            return TextWidth(startIndex, _length - startIndex);
        }

        public float TextWidth(int startIndex, int length)
        {
            int i = 0;
            int consumed = 0;
            for (; i < _hashedParts.Length; i++)
            {
                if (consumed + _hashes[i].VisibleCharLength(_hashedParts[i]) > startIndex)
                {
                    break;
                }

                consumed += _hashes[i].VisibleCharLength(_hashedParts[i]);
            }

            float width = 0;
            for (; i < _hashedParts.Length; i++)
            {
                if (length <= 0)
                {
                    break;
                }

                int start = Math.Max(startIndex - consumed, 0);
                int count = Math.Clamp(length, 0, _hashes[i].VisibleCharLength(_hashedParts[i]) - start);

                width += _hashes[i].GetWidth(_hashedParts[i].Substring(start, count));

                consumed += start + count;
                length -= count;
            }

            return width;
        }

        public void Draw(Vector2 position, Color color, float opacity)
        {
            float xOffset = 0;
            for (int i = 0; i < _hashedParts.Length; i++)
            {
                _hashes[i].Draw(_hashedParts[i], position + new Vector2(xOffset, 0), opacity);

                xOffset += _hashes[i].GetWidth(_hashedParts[i]);
            }
        }
    }
}
