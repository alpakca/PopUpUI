using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

#pragma warning disable 8524

namespace PopUpUI.Text
{
    public static class ParseHelpers
    {
        public static Vector2 GetAlignmentOffset(TextAlignment alignment, Point surfaceSize, Point textSize)
        {
            int xOffset = alignment switch
            {
                TextAlignment.TopLeft or TextAlignment.MiddleLeft or TextAlignment.BottomLeft => 0,
                TextAlignment.TopMiddle or TextAlignment.Center or TextAlignment.BottomMiddle => (surfaceSize.X - textSize.X) / 2,
                TextAlignment.TopRight or TextAlignment.MiddleRight or TextAlignment.BottomRight => surfaceSize.X - textSize.X
            };

            int yOffset = alignment switch
            {
                TextAlignment.TopLeft or TextAlignment.TopMiddle or TextAlignment.TopRight => 0,
                TextAlignment.MiddleLeft or TextAlignment.Center or TextAlignment.MiddleRight => (surfaceSize.Y - textSize.Y) / 2,
                TextAlignment.BottomLeft or TextAlignment.BottomMiddle or TextAlignment.BottomRight => surfaceSize.Y - textSize.Y
            };

            return new Vector2(xOffset, yOffset);
        }

        public static Vector2 GetOrientationOffset(TextOrientation orientation, int horizontalTextLength, float lineWidth)
        {
            return orientation switch
            {
                TextOrientation.Left => new Vector2(),
                TextOrientation.Middle => new Vector2((horizontalTextLength - (int)lineWidth) / 2, 0),
                TextOrientation.Right => new Vector2(horizontalTextLength - (int)lineWidth, 0),
            };
        }

        public static float GetWidth(SpriteFont font, string str)
        {
            return font.MeasureString(str).X;
        }

        public static int AllWordLengths(string[] words)
        {
            return words.Sum(word => word.Length)
                 + words.Length - 1;  // for space characters
        }

        public static int AllWordLengths(HashedTextPiece[] words)
        {
            return words.Sum(word => word.VisibleCharLength)
                 + words.Length - 1; // for space characters
        }

        public static float GetWordSpacing(int horizontalTextLength, float lineWidth, int wordCount)
        {
            return wordCount == 0 ? 0 : (int)((horizontalTextLength - lineWidth) / (wordCount - 1));
        }

        public static int[] AllNewLineIndexes(string text)
        {
            int count = 0;
            Span<int> indexes = stackalloc int[text.Length];

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n') indexes[count++] = i;
            }

            return indexes.Slice(0, count).ToArray();
        }
    }
}
