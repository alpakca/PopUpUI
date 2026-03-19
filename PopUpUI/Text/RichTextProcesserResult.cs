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
        private CaretLayout _caret;

        public Vector2 TextSize => _textSize == default ? _textSize = GetTextSize() : _textSize;
        public int VisibleCharCount => _visibleCharCount == -1 ? _visibleCharCount = GetVisibleCharCount() : _visibleCharCount;

        public int LineCount => _lines.Count;

        public ICaretLayout Caret => _caret ??= new CaretLayout(this);

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

            offset += _data.TextOffset.ToVector2() + ProcesserHelpers.GetAlignmentOffset(_data.Alignment, new Point(target.Width, target.Height), TextSize);

            float spaceWidth = _data.WordSpacing;
            float xOffset = _data.FirstRowOffset;
            foreach ((HashedTextPiece[] line, float lineWidth) in _lines)
            {
                Vector2 lineOffset = ProcesserHelpers.GetOrientationOffset(_data.Orientation, _data.HorizontalTextLength, lineWidth)
                                   + new Vector2(xOffset, 0);

                foreach (HashedTextPiece hashedPiece in line)
                {
                    hashedPiece.DrawHash(offset + lineOffset);
                    lineOffset.X += hashedPiece.TextWidth() + spaceWidth;
                }

                offset.Y += _data.LineSpacing;
                xOffset = 0;
            }

            DrawHelper.EndBatch();
        }

        private Vector2 GetTextSize()
        {
            if (_data.HorizontalTextLength != int.MaxValue)
            {
                return new Vector2(_data.HorizontalTextLength, _data.LineSpacing * _lines.Count);
            }

            return new Vector2((int)_lines.Max(line => line.LineWidth), _data.LineSpacing * _lines.Count);
        }

        public int GetLineVisibleCharCount(int lineIndex)
        {
            if (_lines.Count == 0)
            {
                return 0;
            }

            return ProcesserHelpers.LineVisibleCharCount(_lines[lineIndex].Parts);
        }

        public float GetLineWidth(int lineIndex)
        {
            if (_lines.Count == 0)
            {
                return 0;
            }

            return _lines[lineIndex].LineWidth;
        }

        private int GetVisibleCharCount()
        {
            if (_lines.Count == 0)
            {
                return 0;
            }

            return _visibleCharCount = _lines.Sum(tuple => ProcesserHelpers.LineVisibleCharCount(tuple.Parts) + 1) - 1;
        }

        private class CaretLayout : ICaretLayout
        {
            private readonly RichTextProcesserResult _result;

            private ProcesserData Data => _result._data;
            private List<(HashedTextPiece[] Parts, float LineWidth)> Lines => _result._lines;

            public CaretPosition MinimumPosition => new CaretPosition(0, 0);
            public CaretPosition MaximumPosition => Lines.Count != 0 ? new CaretPosition(Lines.Count - 1, _result.GetLineVisibleCharCount(Lines.Count - 1))
                                                                     : new CaretPosition(0, 0);

            public CaretLayout(RichTextProcesserResult result)
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
                         + ProcesserHelpers.FirstRowOffsetIfFirst(caretPosition.LineIndex, Data.FirstRowOffset)
                         + (targetSize.HasValue ? ProcesserHelpers.GetAlignmentOffset(Data.Alignment, targetSize.Value, _result.TextSize) : default);
                }

                float spaceWidth = Data.WordSpacing;

                // length of words and spaces until the word that caret in
                float xOffset = ProcesserHelpers.WidthUntil(Lines[caretPosition.LineIndex].Parts, spaceWidth, caretPosition.IndexInsideLine);

                return ProcesserHelpers.FirstRowOffsetIfFirst(caretPosition.LineIndex, Data.FirstRowOffset)
                     + ProcesserHelpers.GetScreenCaretPosition(caretPosition, Data, _result.TextSize, xOffset, targetSize);
            }

            public CaretPosition MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize)
            {
                mouseOnTarget -= Data.TextOffset.ToVector2();
                if (targetSize.HasValue)
                {
                    mouseOnTarget -= ProcesserHelpers.GetAlignmentOffset(Data.Alignment, targetSize.Value, _result.TextSize);
                }

                int lineIndex = (int)(mouseOnTarget.Y / Data.LineSpacing);
                int caretLineWordLength = ProcesserHelpers.LineVisibleCharCount(Lines[lineIndex].Parts);

                mouseOnTarget -= ProcesserHelpers.FirstRowOffsetIfFirst(lineIndex, Data.FirstRowOffset)
                               + ProcesserHelpers.GetOrientationOffset(Data.Orientation, Data.HorizontalTextLength, Lines[lineIndex].LineWidth);

                // shortcut for selecting the last character of line if line width is smaller than mouse position
                if (Lines[lineIndex].LineWidth < mouseOnTarget.X)
                {
                    return new CaretPosition(lineIndex, caretLineWordLength);
                }

                int consumedChars = 0;
                float currentWidth = 0;

                float spaceWidth = Data.WordSpacing;
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

                    if (currentWidth + spaceWidth / 2 > mouseOnTarget.X)
                    {
                        return new CaretPosition(lineIndex, consumedChars + part.VisibleCharLength);
                    }

                    currentWidth += spaceWidth;
                    consumedChars += part.VisibleCharLength + 1;
                }

                // should not reach this point - already handled in shortcut
                return new CaretPosition(lineIndex, caretLineWordLength);
            }
        }
    }
}
