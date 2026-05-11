using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;

#pragma warning disable 8524

namespace PopUpUI.Text
{
    public static class ProcesserHelpers
    {
        public static ITextPiece[] ToSpacedPieces(Span<string> words, SpriteFont font, int horizontalTextLength, float wordsWidth)
        {
            if (words.Length == 0)
            {
                return Array.Empty<ITextPiece>();
            }

            ITextPiece[] pieces = new ITextPiece[2 * words.Length - 1];
            SpaceTextPiece space = new SpaceTextPiece(1, GetWordSpacing(horizontalTextLength, wordsWidth, words.Length));

            for (int i = 0; i < words.Length - 1; i++)
            {
                pieces[i * 2] = new StringTextPiece(words[i], font);
                pieces[i * 2 + 1] = space;
            }

            pieces[^1] = new StringTextPiece(words[^1], font);

            return pieces;
        }

        public static ITextPiece[] ToSpacedPieces(Span<ITextPiece> words, int horizontalTextLength, float wordsWidth)
        {
            if (words.Length == 0)
            {
                return Array.Empty<ITextPiece>();
            }

            ITextPiece[] pieces = new ITextPiece[2 * words.Length - 1];
            SpaceTextPiece space = new SpaceTextPiece(1, GetWordSpacing(horizontalTextLength, wordsWidth, words.Length));

            for (int i = 0; i < words.Length - 1; i++)
            {
                pieces[i * 2] = words[i];
                pieces[i * 2 + 1] = space;
            }

            pieces[^1] = words[^1];

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

        public static int SPVisibleCharCount(SplitLine line)
        {
            return line.VisibleCharCount;
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

        public static float SPGetLineWidth(SplitLine line, int startIndex, int length)
        {
            int charConsumed = 0;
            float width = 0;

            foreach (ITextPiece piece in line)
            {
                if (startIndex > charConsumed + piece.VisibleCharLength)
                {
                    charConsumed += piece.VisibleCharLength;
                    continue;
                }
                else if (startIndex > charConsumed)
                {
                    int charsSkipped = startIndex - charConsumed;
                    width += piece.TextWidth(charsSkipped, Math.Clamp(piece.VisibleCharLength - charsSkipped, 0, length));

                    charConsumed += piece.VisibleCharLength;
                    continue;
                }

                if (charConsumed > startIndex + length)
                {
                    break;
                }

                width += piece.TextWidth(0, Math.Min(piece.VisibleCharLength, startIndex + length - charConsumed));

                charConsumed += piece.VisibleCharLength;
            }

            return width;
        }

        public static Vector2 FirstRowOffsetIfFirst(int lineIndex, float firstRowOffset)
        {
            return lineIndex == 0 ? new Vector2(firstRowOffset, 0) : new Vector2();
        }

        public static TextPosition SPAdvanceCaret(TextPosition caretPosition, int amount, ITextProcesserResult result)
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

                newInsideInsideLine += result.Lines.GetLineVisibleCharCount(newLineIndex) + (result.Lines.SplitWithNewLineChar(newLineIndex) ? 1 : 0);
            }

            while (newInsideInsideLine > result.Lines.GetLineVisibleCharCount(newLineIndex))
            {
                newLineIndex++;
                if (newLineIndex >= result.Lines.LineCount)
                {
                    return result.Caret.MaximumPosition;
                }

                newInsideInsideLine -= result.Lines.GetLineVisibleCharCount(newLineIndex - 1) + (result.Lines.SplitWithNewLineChar(newLineIndex - 1) ? 1 : 0);
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

        public static int SPGetTextCaretPosition(TextPosition caretPosition, ILineLayout lineLayout)
        {
            if (lineLayout.LineCount == 1 || caretPosition.LineIndex == 0)
            {
                return caretPosition.IndexInsideLine;
            }

            return lineLayout.GetVisibleCharCount(0, caretPosition.LineIndex) 
                 + caretPosition.IndexInsideLine 
                 + (lineLayout.SplitWithNewLineChar(caretPosition.LineIndex - 1) ? 1 : 0);
        }

        public static TextPosition SPGetCaretPositionFromText(int textCaretPosition, ITextProcesserResult result)
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
                textCaretPosition -= result.Lines.GetLineVisibleCharCount(lineIndex) + (result.Lines.SplitWithNewLineChar(lineIndex) ? 1 : 0);

                lineIndex++;
            }

            return new TextPosition(lineIndex, textCaretPosition);
        }

        public static Vector2 GetScreenCaretPositionOriented(TextPosition caretPosition, ProcesserData data, Vector2 textSize, 
                                                             float xOffset, float lineWidth, Point? targetSize)
        {
            if (targetSize.HasValue)
            {
                return new Vector2(xOffset, data.LineSpacing * caretPosition.LineIndex)
                     + GetOrientationOffset(data.Orientation, data.HorizontalTextLength, lineWidth)
                     + GetAlignmentOffset(data.Alignment, targetSize.Value, textSize);
            }

            return new Vector2(xOffset, data.LineSpacing * caretPosition.LineIndex)
                 + GetOrientationOffset(data.Orientation, data.HorizontalTextLength, lineWidth);
        }
    }
}
