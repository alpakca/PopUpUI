using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

#pragma warning disable 8524

namespace PopUpUI.Text
{
    public static class ProcesserHelpers
    {
        public static Vector2 GetAlignmentOffset(TextAlignment alignment, Point surfaceSize, Vector2 textSize)
        {
            float xOffset = alignment switch
            {
                TextAlignment.TopLeft or TextAlignment.MiddleLeft or TextAlignment.BottomLeft => 0,
                TextAlignment.TopMiddle or TextAlignment.Center or TextAlignment.BottomMiddle => (surfaceSize.X - textSize.X) / 2,
                TextAlignment.TopRight or TextAlignment.MiddleRight or TextAlignment.BottomRight => surfaceSize.X - textSize.X
            };

            float yOffset = alignment switch
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

        public static int LineVisibleCharCount(string[] words)
        {
            return words.Sum(word => word.Length)
                 + words.Length - 1;  // for space characters
        }

        public static int LineVisibleCharCount(HashedTextPiece[] words)
        {
            return words.Sum(word => word.VisibleCharLength)
                 + words.Length - 1; // for space characters
        }

        public static float GetWordSpacing(int horizontalTextLength, float totalWordsWidth, int wordCount)
        {
            return wordCount == 0 ? 0 : (int)((horizontalTextLength - totalWordsWidth) / (wordCount - 1));
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

        public static float GetLineWidth(string[] pieces, SpriteFont font, float wordSpacing, int startIndex, int length)
        {
            int charConsumed = 0;
            float width = 0;
            int passedWordCount = 0;

            for (int i = 0; i < pieces.Length; i++)
            {
                if (startIndex > charConsumed + pieces[i].Length)
                {
                    charConsumed += pieces[i].Length + 1;
                    continue;
                }
                else if (startIndex > charConsumed)
                {
                    int charsSkipped = startIndex - charConsumed;
                    width += GetWidth(font, pieces[i].Substring(charsSkipped, Math.Clamp(pieces[i].Length - charsSkipped, 0, length)));

                    passedWordCount++;
                    charConsumed += pieces[i].Length + 1;
                    continue;
                }

                if (charConsumed > startIndex + length)
                {
                    break;
                }

                width += GetWidth(font, pieces[i].Substring(0, Math.Min(pieces[i].Length, startIndex + length - charConsumed)));

                passedWordCount++;
                charConsumed += pieces[i].Length + 1;
            }

            width += passedWordCount != 0 ? (passedWordCount - 1) * wordSpacing : 0;

            return width;
        }

        public static float GetLineWidth(HashedTextPiece[] pieces, float wordSpacing, int startIndex, int length)
        {
            int charConsumed = 0;
            float width = 0;
            int passedWordCount = 0;

            for (int i = 0; i < pieces.Length; i++)
            {
                if (startIndex > charConsumed + pieces[i].VisibleCharLength)
                {
                    charConsumed += pieces[i].VisibleCharLength + 1;
                    continue;
                }
                else if (startIndex > charConsumed)
                {
                    int charsSkipped = startIndex - charConsumed;
                    width += pieces[i].TextWidth(charsSkipped, Math.Clamp(pieces[i].VisibleCharLength - charsSkipped, 0, length));

                    passedWordCount++;
                    charConsumed += pieces[i].VisibleCharLength + 1;
                    continue;
                }

                if (charConsumed > startIndex + length)
                {
                    break;
                }

                width += pieces[i].TextWidth(0, Math.Min(pieces[i].VisibleCharLength, startIndex + length - charConsumed));

                passedWordCount++;
                charConsumed += pieces[i].VisibleCharLength + 1;
            }

            width += passedWordCount != 0 ? (passedWordCount - 1) * wordSpacing : 0;

            return width;
        }

        public static float WidthUntil(HashedTextPiece[] pieces, float wordSpacing, int index)
        {
            int charConsumed = 0;
            float width = 0;

            int i = 0;
            for (; i < pieces.Length; i++)
            {
                if (charConsumed > index)
                {
                    break;
                }

                width += pieces[i].TextWidth(0, Math.Min(pieces[i].VisibleCharLength, index - charConsumed));
                charConsumed += pieces[i].VisibleCharLength + 1;
            }

            width += i != 0 ? (i - 1) * wordSpacing : 0;

            return width;
        }

        public static Vector2 FirstRowOffsetIfFirst(int lineIndex, float firstRowOffset)
        {
            return lineIndex == 0 ? new Vector2(firstRowOffset, 0) : new Vector2();
        }

        public static TextPosition AdvanceCaret(TextPosition caretPosition, int amount, ITextProcesserResult result)
        {
            int newLineIndex = caretPosition.LineIndex;
            int newInsideInsideLine = caretPosition.IndexInsideLine + amount;

            while (newInsideInsideLine < 0)
            {
                newLineIndex--;
                if (newLineIndex < 0)
                {
                    return result.Caret.MinimumPosition;
                }

                newInsideInsideLine += result.Lines.GetLineVisibleCharCount(newLineIndex) + 1;
            }

            while (newInsideInsideLine > result.Lines.GetLineVisibleCharCount(newLineIndex))
            {
                newLineIndex++;
                if (newLineIndex >= result.Lines.LineCount)
                {
                    return result.Caret.MaximumPosition;
                }

                newInsideInsideLine -= result.Lines.GetLineVisibleCharCount(newLineIndex - 1) + 1;
            }

            return new TextPosition(newLineIndex, newInsideInsideLine);
        }

        public static TextPosition ClampCaret(TextPosition caretPosition, ITextProcesserResult result)
        {
            int newLineIndex = 0;
            if (result.Lines.LineCount != 0)
            {
                newLineIndex = Math.Clamp(caretPosition.LineIndex, 0, result.Lines.LineCount - 1);
            }

            return new TextPosition(newLineIndex, Math.Clamp(caretPosition.IndexInsideLine, 0, result.Lines.GetLineVisibleCharCount(newLineIndex)));
        }

        public static TextPosition GetCaretPositionFromText(int textCaretPosition, ITextProcesserResult result)
        {
            if (textCaretPosition < 0)
            {
                return result.Caret.MinimumPosition;
            }
            else if (textCaretPosition > result.Lines.VisibleCharCount)
            {
                return result.Caret.MaximumPosition;
            }

            int lineIndex = 0;
            while (textCaretPosition > result.Lines.GetLineVisibleCharCount(lineIndex))
            {
                lineIndex++;
                textCaretPosition -= result.Lines.GetLineVisibleCharCount(lineIndex - 1) + 1;
            }

            return new TextPosition(lineIndex, textCaretPosition);
        }

        public static Vector2 GetScreenCaretPosition(TextPosition caretPosition, ProcesserData data, Vector2 textSize, float xOffset, Point? targetSize)
        {
            if (targetSize.HasValue)
            {
                return new Vector2(xOffset, data.LineSpacing * caretPosition.LineIndex)
                     + data.TextOffset.ToVector2()
                     + ProcesserHelpers.GetAlignmentOffset(data.Alignment, targetSize.Value, textSize);
            }

            return new Vector2(xOffset, data.LineSpacing * caretPosition.LineIndex)
                 + data.TextOffset.ToVector2();
        }

        public static Vector2 GetScreenCaretPosition(TextPosition caretPosition, ProcesserData data, Vector2 textSize, float xOffset, float lineWidth, Point? targetSize)
        {
            if (targetSize.HasValue)
            {
                return new Vector2(xOffset, data.LineSpacing * caretPosition.LineIndex)
                     + data.TextOffset.ToVector2()
                     + ProcesserHelpers.GetOrientationOffset(data.Orientation, data.HorizontalTextLength, lineWidth)
                     + ProcesserHelpers.GetAlignmentOffset(data.Alignment, targetSize.Value, textSize);
            }

            return new Vector2(xOffset, data.LineSpacing * caretPosition.LineIndex)
                 + data.TextOffset.ToVector2()
                 + ProcesserHelpers.GetOrientationOffset(data.Orientation, data.HorizontalTextLength, lineWidth);
        }
    }
}
