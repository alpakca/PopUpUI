using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PopUpUI.Text
{
    public class TextProcesserResult : ITextProcesserResult
    {
        private readonly ProcesserData _data;
        private readonly List<(string Line, float LineWidth)> _lines;

        private Vector2 _textSize;
        private int _visibleCharCount = -1;
        private CaretLayout _caret;

        public Vector2 TextSize => _textSize == default ? _textSize = GetTextSize() : _textSize;

        public int LineCount => _lines.Count;
        public int VisibleCharCount => _visibleCharCount == -1 ? _visibleCharCount = GetVisibleCharCount() : _visibleCharCount;

        public ICaretLayout Caret => _caret ??= new CaretLayout(this);

        internal TextProcesserResult(List<(string Line, float LineWidth)> lines, ProcesserData data)
        {
            _lines = lines;
            _data = data;
        }

        public void Render(RenderTarget2D target, Vector2 offset)
        {
            // right or middle orientations will cause a too large x offset that text won't appear, with maximum horizontal text length
            if (_data.Orientation != TextOrientation.Left && _data.HorizontalTextLength == int.MaxValue)
            {
                throw new ArgumentException("Maximum HorizontalTextLength should only be used with TextOrientation.Left");
            }

            offset += _data.TextOffset.ToVector2() + ProcesserHelpers.GetAlignmentOffset(_data.Alignment, new Point(target.Width, target.Height), TextSize);

            DrawHelper.SetBeginBatch(target, _data.DrawOptions);

            float xOffset = _data.FirstRowOffset;
            foreach ((string line, float lineWidth) in _lines)
            {
                DrawHelper.Batch.DrawString(_data.Font, line, 
                                            new Vector2(xOffset, 0) 
                                            + offset 
                                            + ProcesserHelpers.GetOrientationOffset(_data.Orientation, _data.HorizontalTextLength, lineWidth), 
                                            _data.TextColor * _data.Opacity);

                xOffset = 0;
                offset.Y += _data.LineSpacing;
            }

            DrawHelper.EndBatch();
        }

        public int GetLineVisibleCharCount(int lineIndex)
        {
            if (_lines.Count == 0)
            {
                return 0;
            }

            return _lines[lineIndex].Line.Length;
        }

        public float GetLineWidth(int lineIndex)
        {
            if (_lines.Count == 0)
            {
                return 0;
            }

            return _lines[lineIndex].LineWidth;
        }

        private Vector2 GetTextSize()
        {
            if (_data.HorizontalTextLength != int.MaxValue)
            {
                return new Vector2(_data.HorizontalTextLength, _data.LineSpacing * _lines.Count);
            }

            return new Vector2((int)_lines.Max(tuple => tuple.LineWidth), _data.LineSpacing * _lines.Count);
        }

        private int GetVisibleCharCount()
        {
            if (_lines.Count == 0)
            {
                return 0;
            }

            return _lines.Sum(tuple => tuple.Line.Length + 1) - 1;
        }

        private class CaretLayout : ICaretLayout
        {
            private readonly TextProcesserResult _result;

            private ProcesserData Data => _result._data;
            private List<(string Line, float LineWidth)> Lines => _result._lines;

            public CaretPosition MinimumPosition => new CaretPosition(0, 0);
            public CaretPosition MaximumPosition => Lines.Count != 0 ? new CaretPosition(Lines.Count - 1, _result.GetLineVisibleCharCount(Lines.Count - 1))
                                                                     : new CaretPosition(0, 0);

            public CaretLayout(TextProcesserResult result)
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
                return Lines.Take(caretPosition.LineIndex).Sum(tuple => tuple.Line.Length + 1)
                     + caretPosition.IndexInsideLine;
            }

            public CaretPosition GetCaretPosition(int textCaretPosition)
            {
                return ProcesserHelpers.GetCaretPositionFromText(textCaretPosition, _result);
            }

            public Vector2 ScreenCaretPosition(CaretPosition caretPosition, Point? targetSize)
            {
                return ProcesserHelpers.FirstRowOffsetIfFirst(caretPosition.LineIndex, Data.FirstRowOffset)
                    + ProcesserHelpers.GetScreenCaretPosition(
                    caretPosition,
                    Data,
                    _result.TextSize,
                    ProcesserHelpers.GetWidth(Data.Font, Lines[caretPosition.LineIndex].Line.Substring(0, caretPosition.IndexInsideLine)),
                    targetSize);
            }

            public CaretPosition MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize)
            {
                mouseOnTarget -= Data.TextOffset.ToVector2();
                if (targetSize.HasValue)
                {
                    mouseOnTarget -= ProcesserHelpers.GetAlignmentOffset(Data.Alignment, targetSize.Value, _result.TextSize);
                }

                int lineIndex = (int)(mouseOnTarget.Y / Data.LineSpacing);

                mouseOnTarget -= ProcesserHelpers.FirstRowOffsetIfFirst(lineIndex, Data.FirstRowOffset)
                               + ProcesserHelpers.GetOrientationOffset(Data.Orientation, Data.HorizontalTextLength, Lines[lineIndex].LineWidth);

                // shortcut for selecting the last character of line if line width is smaller than mouse position
                if (Lines[lineIndex].LineWidth < mouseOnTarget.X)
                {
                    return new CaretPosition(lineIndex, Lines[lineIndex].Line.Length);
                }

                for (int i = 0; i < Lines[lineIndex].Line.Length; i++)
                {
                    // also add adjustment for half of the char length
                    if (ProcesserHelpers.GetWidth(Data.Font, Lines[lineIndex].Line.Substring(0, i))
                      + ProcesserHelpers.GetWidth(Data.Font, Lines[lineIndex].Line[i].ToString()) / 2 > mouseOnTarget.X)
                    {
                        return new CaretPosition(lineIndex, i);
                    }
                }

                // should not reach this point - already handled in shortcut part
                return new CaretPosition(lineIndex, Lines[lineIndex].Line.Length);
            }
        }
    }
}
    