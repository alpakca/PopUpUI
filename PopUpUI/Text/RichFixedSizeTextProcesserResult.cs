using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PopUpUI.Text
{
    public class RichFixedSizeTextProcesserResult : ITextProcesserResult
    {
        private readonly ProcesserData _data;
        private readonly List<(HashedTextPiece[] Parts, float TotalWordsWidth)> _lines;

        private int _visibleCharCount = -1;
        private CaretLayout _caret;

        public Vector2 TextSize => new Vector2(_data.HorizontalTextLength, _data.LineSpacing * _lines.Count);

        public int LineCount => _lines.Count;
        public int VisibleCharCount => _visibleCharCount == -1 ? _visibleCharCount = GetVisibleCharCount() : _visibleCharCount;

        public ICaretLayout Caret => _caret ??= new CaretLayout(this);

        internal RichFixedSizeTextProcesserResult(List<(HashedTextPiece[] Parts, float TotalWordsWidth)> lines, ProcesserData data)
        {
            _data = data;
            _lines = lines;
        }

        public void Render(RenderTarget2D target, Vector2 offset)
        {
            DrawHelper.SetBeginBatch(target, _data.DrawOptions);

            offset += _data.TextOffset.ToVector2() + ProcesserHelpers.GetAlignmentOffset(_data.Alignment, new Point(target.Width, target.Height), TextSize);

            foreach ((HashedTextPiece[] line, float lineWidth) in _lines)
            {
                float xOffset = 0;

                float wordSpacing = ProcesserHelpers.GetWordSpacing(_data.HorizontalTextLength, lineWidth, line.Length);
                foreach (HashedTextPiece hashedPiece in line)
                {
                    hashedPiece.DrawHash(offset + new Vector2(xOffset, 0));
                    xOffset += hashedPiece.TextWidth() + wordSpacing;
                }

                offset.Y += _data.LineSpacing;
            }

            DrawHelper.EndBatch();
        }

        public int GetLineVisibleCharCount(int lineIndex)
        {
            return ProcesserHelpers.LineVisibleCharCount(_lines[lineIndex].Parts);
        }

        public float GetLineWidth(int lineIndex)
        {
            return _data.HorizontalTextLength;
        }

        private int GetVisibleCharCount()
        {
            if (_lines.Count == 0)
            {
                return 0;
            }

            return _visibleCharCount = _lines.Sum(tuple => tuple.Parts.Sum((Func<HashedTextPiece, int>)(part => part.VisibleCharLength + 1))) - 1;
        }

        private class CaretLayout : ICaretLayout
        {
            private readonly RichFixedSizeTextProcesserResult _result;

            private ProcesserData Data => _result._data;
            private List<(HashedTextPiece[] Parts, float TotalWordsWidth)> Lines => _result._lines;

            public CaretPosition MinimumPosition => new CaretPosition(0, 0);
            public CaretPosition MaximumPosition => new CaretPosition(Lines.Count - 1, _result.GetLineVisibleCharCount(Lines.Count - 1));

            public CaretLayout(RichFixedSizeTextProcesserResult result)
            {
                _result = result;
            }

            public CaretPosition Advance(CaretPosition caretPosition, int amount)
            {
                return ProcesserHelpers.AdvanceCaret(caretPosition, amount, _result);
            }

            public CaretPosition Clamp(CaretPosition caretPosition)
            {
                return ProcesserHelpers.ClampCaret(caretPosition, _result);
            }

            public CaretPosition Clamp(CaretPosition caretPosition, int lineIndexOffset, int indexInsideLineOffset)
            {
                return Clamp(new CaretPosition(caretPosition.LineIndex + lineIndexOffset,
                                               caretPosition.IndexInsideLine + indexInsideLineOffset));
            }

            public int GetTextCaretPosition(CaretPosition caretPosition)
            {
                return Lines.Take(caretPosition.LineIndex).Sum(tuple => ProcesserHelpers.LineVisibleCharCount(tuple.Parts) + 1) // +1 for new line char
                     + caretPosition.IndexInsideLine;
            }

            public CaretPosition GetCaretPosition(int textCaretPosition)
            {
                return ProcesserHelpers.GetCaretPositionFromText(textCaretPosition, _result);
            }

            public Vector2 ScreenCaretPosition(CaretPosition caretPosition, Point? targetSize)
            {
                if (Lines.Count == 0)
                {
                    return Data.TextOffset.ToVector2()
                         + (targetSize.HasValue ? ProcesserHelpers.GetAlignmentOffset(Data.Alignment, targetSize.Value, _result.TextSize) : default);
                }

                float wordSpacing = ProcesserHelpers.GetWordSpacing(Data.HorizontalTextLength, 
                                                                    Lines[caretPosition.LineIndex].TotalWordsWidth, 
                                                                    Lines[caretPosition.LineIndex].Parts.Length);

                // length of words and spaces until the word that caret in
                float xOffset = ProcesserHelpers.WidthUntil(Lines[caretPosition.LineIndex].Parts, wordSpacing, caretPosition.IndexInsideLine);

                return ProcesserHelpers.GetScreenCaretPosition(caretPosition, Data, _result.TextSize, xOffset, targetSize);
            }

            public CaretPosition MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize)
            {
                if (targetSize.HasValue)
                {
                    mouseOnTarget -= ProcesserHelpers.GetAlignmentOffset(Data.Alignment, targetSize.Value, _result.TextSize);
                }

                mouseOnTarget -= Data.TextOffset.ToVector2();

                int lineIndex = (int)(mouseOnTarget.Y / Data.LineSpacing);
                float wordSpacing = ProcesserHelpers.GetWordSpacing(Data.HorizontalTextLength, Lines[lineIndex].TotalWordsWidth, Lines[lineIndex].Parts.Length);

                int charsUntilLine = Lines.Take(lineIndex).Sum(tuple => ProcesserHelpers.LineVisibleCharCount(tuple.Parts) + 1); // +1 for newline chars
                int caretLineWordLength = ProcesserHelpers.LineVisibleCharCount(Lines[lineIndex].Parts);

                // shortcut for selecting the last character of line if line width is smaller than mouse position
                if (Lines[lineIndex].TotalWordsWidth + wordSpacing * (Lines[lineIndex].Parts.Length - 1) < mouseOnTarget.X)
                {
                    return new CaretPosition(lineIndex, caretLineWordLength);
                }

                int consumedChars = 0;
                float currentWidth = 0;

                foreach (var part in Lines[lineIndex].Parts)
                {
                    for (int i = 0; i < part.VisibleCharLength; i++)
                    {
                        if (currentWidth + part.TextWidth(0, i) + part.TextWidth(i, 1) / 2 > mouseOnTarget.X)
                        {
                            return new CaretPosition(lineIndex, consumedChars + i);
                        }
                    }

                    currentWidth += part.TextWidth();

                    if (currentWidth + wordSpacing / 2 > mouseOnTarget.X)
                    {
                        return new CaretPosition(lineIndex, consumedChars + part.VisibleCharLength);
                    }

                    currentWidth += wordSpacing;
                    consumedChars += part.VisibleCharLength + 1;
                }

                // should not reach this point - already handled in shortcut
                return new CaretPosition(lineIndex, caretLineWordLength);
            }
        }
    }
}
