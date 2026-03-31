using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

#pragma warning disable 8524

namespace PopUpUI.Text
{
    public static class ProcesserHelpers
    {
        public static StringTextPiece[] ToStringPiece(Span<string> words, SpriteFont font)
        {
            StringTextPiece[] pieces = new StringTextPiece[words.Length];
            for (int i = 0; i < words.Length; i++)
            {
                pieces[i] = new StringTextPiece(words[i], font);
            }

            return pieces;
        }

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

        public static int WordsVisibleCharCount(ITextPiece[] words)
        {
            return words.Sum(word => word.VisibleCharLength)
                 + words.Length - 1; // for space characters between words
        }

        public static int PartsVisibleCharCount(ITextPiece[] parts)
        {
            return parts.Sum(part => part.VisibleCharLength);
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

        public static float GetWordsLineWidth(ITextPiece[] words, float wordSpacing, int startIndex, int length)
        {
            int charConsumed = 0;
            float width = 0;
            int passedWordCount = 0;

            for (int i = 0; i < words.Length; i++)
            {
                if (startIndex > charConsumed + words[i].VisibleCharLength)
                {
                    charConsumed += words[i].VisibleCharLength + 1;
                    continue;
                }
                else if (startIndex > charConsumed)
                {
                    int charsSkipped = startIndex - charConsumed;
                    width += words[i].TextWidth(charsSkipped, Math.Clamp(words[i].VisibleCharLength - charsSkipped, 0, length));

                    passedWordCount++;
                    charConsumed += words[i].VisibleCharLength + 1;
                    continue;
                }

                if (charConsumed > startIndex + length)
                {
                    break;
                }

                width += words[i].TextWidth(0, Math.Min(words[i].VisibleCharLength, startIndex + length - charConsumed));

                passedWordCount++;
                charConsumed += words[i].VisibleCharLength + 1;
            }

            width += passedWordCount != 0 ? (passedWordCount - 1) * wordSpacing : 0;

            return width;
        }

        public static float GetPartsLineWidth(ITextPiece[] parts, int startIndex, int length)
        {
            int charConsumed = 0;
            float width = 0;

            for (int i = 0; i < parts.Length; i++)
            {
                if (startIndex > charConsumed + parts[i].VisibleCharLength)
                {
                    charConsumed += parts[i].VisibleCharLength;
                    continue;
                }
                else if (startIndex > charConsumed)
                {
                    int charsSkipped = startIndex - charConsumed;
                    width += parts[i].TextWidth(charsSkipped, Math.Clamp(parts[i].VisibleCharLength - charsSkipped, 0, length));

                    charConsumed += parts[i].VisibleCharLength;
                    continue;
                }

                if (charConsumed > startIndex + length)
                {
                    break;
                }

                width += parts[i].TextWidth(0, Math.Min(parts[i].VisibleCharLength, startIndex + length - charConsumed));

                charConsumed += parts[i].VisibleCharLength;
            }

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

        public static int GetTextCaretPosition(TextPosition caretPosition, ILineLayout lineLayout)
        {
            // +1 for newline char for the last line before caretPosition.LineIndex
            return lineLayout.GetVisibleCharCount(0, caretPosition.LineIndex) + 1 + caretPosition.IndexInsideLine;
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
                     + GetAlignmentOffset(data.Alignment, targetSize.Value, textSize);
            }

            return new Vector2(xOffset, data.LineSpacing * caretPosition.LineIndex)
                 + data.TextOffset.ToVector2();
        }

        public static Vector2 GetScreenCaretPositionOriented(TextPosition caretPosition, ProcesserData data, Vector2 textSize, 
                                                             float xOffset, float lineWidth, Point? targetSize)
        {
            if (targetSize.HasValue)
            {
                return new Vector2(xOffset, data.LineSpacing * caretPosition.LineIndex)
                     + data.TextOffset.ToVector2()
                     + GetOrientationOffset(data.Orientation, data.HorizontalTextLength, lineWidth)
                     + GetAlignmentOffset(data.Alignment, targetSize.Value, textSize);
            }

            return new Vector2(xOffset, data.LineSpacing * caretPosition.LineIndex)
                 + data.TextOffset.ToVector2()
                 + GetOrientationOffset(data.Orientation, data.HorizontalTextLength, lineWidth);
        }
    }
}
