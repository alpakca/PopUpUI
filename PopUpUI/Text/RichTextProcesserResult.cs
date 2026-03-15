using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PopUpUI.Text
{
    public class RichTextProcesserResult : ITextProcesserResult
    {
        private readonly ProcesserData _data;
        private readonly List<(HashedTextPiece[] Parts, float LineWidth)> _lines;

        private int _visibleCharCount = -1;
        private Vector2 _textSize;

        internal RichTextProcesserResult(List<(HashedTextPiece[] Parts, float LineWidth)> lines, ProcesserData data)
        {
            _data = data;
            _lines = lines;
        }

        public void Render(RenderTarget2D target, Vector2 offset)
        {
            // right or middle orientations will cause a too large x offset that text won't appear, with maximum horizontal text length
            if (_data.Orientation != TextOrientation.Left && _data.HorizontalTextLength == int.MaxValue)
            {
                throw new ArgumentException("Maximum HorizontalTextLength should only be used with TextOrientation.Left");
            }

            DrawHelper.SetBeginBatch(target, _data.DrawOptions);

            offset += _data.TextOffset.ToVector2() + ParseHelpers.GetAlignmentOffset(_data.Alignment, new Point(target.Width, target.Height), TextSizeFromHashedLines());

            float spaceWidth = _data.WordSpacing;
            foreach ((HashedTextPiece[] line, float lineWidth) in _lines)
            {
                Vector2 lineOffset = ParseHelpers.GetOrientationOffset(_data.Orientation, _data.HorizontalTextLength, lineWidth);

                foreach (HashedTextPiece hashedPiece in line)
                {
                    hashedPiece.DrawHash(offset + lineOffset);
                    lineOffset.X += hashedPiece.TextWidth() + spaceWidth;
                }

                offset.Y += _data.LineSpacing;
            }

            DrawHelper.EndBatch();
        }

        private Point TextSizeFromHashedLines()
        {
            if (_data.HorizontalTextLength != int.MaxValue)
            {
                return new Point(_data.HorizontalTextLength, _data.LineSpacing * _lines.Count);
            }

            return new Point((int)_lines.Max(line => line.LineWidth), _data.LineSpacing * _lines.Count);
        }

        public Vector2 GetTextSize()
        {
            if (_textSize == Vector2.Zero)
            {
                _textSize = TextSizeFromHashedLines().ToVector2();
            }

            return _textSize;
        }

        public int GetVisibleCharCount()
        {
            if (_lines.Count == 0)
            {
                return 0;
            }

            if (_visibleCharCount == -1)
            {
                _visibleCharCount = _lines.Sum(tuple => tuple.Parts.Sum((Func<HashedTextPiece, int>)(part => part.VisibleCharLength + 1))) - 1;
            }

            return _visibleCharCount;
        }

        public Vector2 ScreenCaretPosition(int caretPosition, Point? targetSize)
        {
            if (_lines.Count == 0)
            {
                return _data.TextOffset.ToVector2()
                     + (targetSize.HasValue ? ParseHelpers.GetAlignmentOffset(_data.Alignment, targetSize.Value, TextSizeFromHashedLines()) : default);
            }

            int charConsumed = 0;

            int lineIndex = 0;
            int wordIndex = -1;
            foreach (var (lineWords, _) in _lines)
            {
                for (int i = 0; i < lineWords.Length; i++)
                {
                    if (caretPosition <= charConsumed + lineWords[i].VisibleCharLength)
                    {
                        wordIndex = i;
                        break;
                    }

                    // +1 for space char, or new line char if it is the last word of line
                    charConsumed += lineWords[i].VisibleCharLength + 1;
                }

                if (wordIndex != -1)
                {
                    break;
                }

                lineIndex++;
            }

            float spaceWidth = _data.WordSpacing;

            // length of words and spaces until the word that caret in
            float xOffset = _lines[lineIndex].Parts.Take(wordIndex).Sum(part => part.TextWidth() + spaceWidth);

            // length of word until caret position
            xOffset += _lines[lineIndex].Parts[wordIndex].TextWidth(0, caretPosition - charConsumed);

            if (targetSize.HasValue)
            {
                return new Vector2(xOffset, _data.LineSpacing * lineIndex)
                     + _data.TextOffset.ToVector2()
                     + ParseHelpers.GetOrientationOffset(_data.Orientation, _data.HorizontalTextLength, _lines[lineIndex].LineWidth)
                     + ParseHelpers.GetAlignmentOffset(_data.Alignment, targetSize.Value, TextSizeFromHashedLines());
            }

            return new Vector2(xOffset, _data.LineSpacing * lineIndex)
                 + _data.TextOffset.ToVector2()
                 + ParseHelpers.GetOrientationOffset(_data.Orientation, _data.HorizontalTextLength, _lines[lineIndex].LineWidth);
        }

        public int MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize)
        {
            if (_lines.Count == 0)
            {
                return 0;
            }

            if (targetSize.HasValue)
            {
                mouseOnTarget -= ParseHelpers.GetAlignmentOffset(_data.Alignment, targetSize.Value, TextSizeFromHashedLines());
            }

            mouseOnTarget -= _data.TextOffset.ToVector2();

            int lineIndex = (int)(mouseOnTarget.Y / _data.LineSpacing);

            int charsUntilLine = _lines.Take(lineIndex).Sum(tuple => ParseHelpers.AllWordLengths(tuple.Parts) + 1); // +1 for newline chars
            int caretLineWordLength = ParseHelpers.AllWordLengths(_lines[lineIndex].Parts);

            mouseOnTarget -= ParseHelpers.GetOrientationOffset(_data.Orientation, _data.HorizontalTextLength, _lines[lineIndex].LineWidth);

            // shortcut for selecting the last character of line if line width is smaller than mouse position
            if (_lines[lineIndex].LineWidth < mouseOnTarget.X)
            {
                return charsUntilLine + caretLineWordLength;
            }

            int consumedChars = 0;
            float currentWidth = 0;

            float spaceWidth = _data.WordSpacing;
            foreach (var part in _lines[lineIndex].Parts)
            {
                for (int i = 0; i < part.VisibleCharLength; i++)
                {
                    if (currentWidth + part.TextWidth(0, i) + part.TextWidth(i, 1) / 2 > mouseOnTarget.X)
                    {
                        return charsUntilLine + consumedChars + i;
                    }
                }

                currentWidth += part.TextWidth();

                if (currentWidth + spaceWidth / 2 > mouseOnTarget.X)
                {
                    return charsUntilLine + consumedChars + part.VisibleCharLength;
                }

                currentWidth += spaceWidth;
                consumedChars += part.VisibleCharLength + 1;
            }

            // should not reach this point - already handled in shortcut
            return charsUntilLine + caretLineWordLength;
        }
    }
}
