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
        private readonly List<(ITextPiece[] Parts, float LineWidth)> _lines;

        private Vector2 _textSize;
        private LineLayout _lineLayout;
        private CaretLayout _caret;

        public ILineLayout Lines => _lineLayout ??= new LineLayout(this);
        public ICaretLayout Caret => _caret ??= new CaretLayout(this);

        public Vector2 TextSize => _textSize == default ? _textSize = GetTextSize() : _textSize;

        internal TextProcesserResult(List<(ITextPiece[] Parts, float LineWidth)> lines, ProcesserData data)
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
            foreach ((ITextPiece[] line, float lineWidth) in _lines)
            {
                Vector2 lineOffset = ProcesserHelpers.GetOrientationOffset(_data.Orientation, _data.HorizontalTextLength, lineWidth)
                                   + new Vector2(xOffset, 0);

                foreach (ITextPiece hashedPart in line)
                {
                    hashedPart.Draw(offset + lineOffset, _data.TextColor, _data.Opacity);
                    lineOffset.X += hashedPart.TextWidth();
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

            return new Vector2((int)_lines.Max(tuple => tuple.LineWidth), _data.LineSpacing * _lines.Count);
        }

        private class LineLayout : ILineLayout
        {
            private int _visibleCharCount = -1;
            private readonly TextProcesserResult _result;

            private ProcesserData Data => _result._data;
            private List<(ITextPiece[] Parts, float LineWidth)> Lines => _result._lines;

            public int LineCount => Lines.Count;
            public int VisibleCharCount => _visibleCharCount == -1 ? _visibleCharCount = GetVisibleCharCount() : _visibleCharCount;

            public LineLayout(TextProcesserResult result)
            {
                _result = result;
            }

            private int GetVisibleCharCount()
            {
                if (Lines.Count == 0)
                {
                    return 0;
                }

                return GetVisibleCharCount(0, Lines.Count);
            }

            public int GetVisibleCharCount(int startLineIndex, int lineCount)
            {
                int count = 0;
                while (lineCount-- > 0)
                {
                    count += ProcesserHelpers.PartsVisibleCharCount(Lines[startLineIndex].Parts) + 1; // +1 newline char

                    startLineIndex++;
                }

                return count - 1; // remove the addition newline at the end
            }

            public int GetLineVisibleCharCount(int lineIndex)
            {
                if (Lines.Count == 0)
                {
                    return 0;
                }

                return ProcesserHelpers.PartsVisibleCharCount(Lines[lineIndex].Parts);
            }

            public float GetLineWidth(int lineIndex)
            {
                if (Lines.Count == 0)
                {
                    return 0;
                }

                return Lines[lineIndex].LineWidth;
            }

            public float GetLineWidth(TextPosition startPosition)
            {
                if (Lines.Count == 0)
                {
                    return 0;
                }

                return ProcesserHelpers.GetPartsLineWidth(Lines[startPosition.LineIndex].Parts,
                                                          startPosition.IndexInsideLine,
                                                          GetLineVisibleCharCount(startPosition.LineIndex) - startPosition.IndexInsideLine);
            }

            public float GetLineWidth(TextPosition startPosition, int length)
            {
                if (Lines.Count == 0)
                {
                    return 0;
                }

                return ProcesserHelpers.GetPartsLineWidth(Lines[startPosition.LineIndex].Parts, startPosition.IndexInsideLine, length);
            }
        }

        private class CaretLayout : ICaretLayout
        {
            private readonly TextProcesserResult _result;

            private ProcesserData Data => _result._data;
            private List<(ITextPiece[] Parts, float LineWidth)> Lines => _result._lines;

            public TextPosition MinimumPosition => new TextPosition(0, 0);
            public TextPosition MaximumPosition => Lines.Count != 0 ? new TextPosition(Lines.Count - 1, _result.Lines.GetLineVisibleCharCount(Lines.Count - 1))
                                                                    : new TextPosition(0, 0);

            public CaretLayout(TextProcesserResult result)
            {
                _result = result;
            }

            public TextPosition Advance(TextPosition caretPosition, int amount)
            {
                return ProcesserHelpers.AdvanceCaret(caretPosition, amount, _result);
            }

            public TextPosition Clamp(TextPosition caretPosition)
            {
                return ProcesserHelpers.ClampCaret(caretPosition, _result);
            }

            public TextPosition Clamp(TextPosition caretPosition, int lineIndexOffset, int indexInsideLineOffset)
            {
                return Clamp(new TextPosition(caretPosition.LineIndex + lineIndexOffset,
                                               caretPosition.IndexInsideLine + indexInsideLineOffset));
            }

            public int GetTextCaretPosition(TextPosition caretPosition)
            {
                return ProcesserHelpers.GetTextCaretPosition(caretPosition, _result.Lines);
            }

            public TextPosition GetCaretPosition(int textCaretPosition)
            {
                return ProcesserHelpers.GetCaretPositionFromText(textCaretPosition, _result);
            }

            public Vector2 ScreenCaretPosition(TextPosition caretPosition, Point? targetSize)
            {
                if (Lines.Count == 0)
                {
                    return Data.TextOffset.ToVector2()
                         + ProcesserHelpers.FirstRowOffsetIfFirst(caretPosition.LineIndex, Data.FirstRowOffset)
                         + (targetSize.HasValue ? ProcesserHelpers.GetAlignmentOffset(Data.Alignment, targetSize.Value, _result.TextSize) : default);
                }

                // length of words and spaces until the word that caret in
                float xOffset = ProcesserHelpers.GetPartsLineWidth(Lines[caretPosition.LineIndex].Parts, 0, caretPosition.IndexInsideLine);

                return ProcesserHelpers.FirstRowOffsetIfFirst(caretPosition.LineIndex, Data.FirstRowOffset)
                     + ProcesserHelpers.GetScreenCaretPositionOriented(caretPosition, Data, _result.TextSize, xOffset, Lines[caretPosition.LineIndex].LineWidth, targetSize);
            }

            public TextPosition MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize)
            {
                mouseOnTarget -= Data.TextOffset.ToVector2();
                if (targetSize.HasValue)
                {
                    mouseOnTarget -= ProcesserHelpers.GetAlignmentOffset(Data.Alignment, targetSize.Value, _result.TextSize);
                }

                int lineIndex = (int)(mouseOnTarget.Y / Data.LineSpacing);
                int caretLinePartLength = ProcesserHelpers.PartsVisibleCharCount(Lines[lineIndex].Parts);

                mouseOnTarget -= ProcesserHelpers.FirstRowOffsetIfFirst(lineIndex, Data.FirstRowOffset)
                               + ProcesserHelpers.GetOrientationOffset(Data.Orientation, Data.HorizontalTextLength, Lines[lineIndex].LineWidth);

                // shortcut for selecting the last character of line if line width is smaller than mouse position
                if (Lines[lineIndex].LineWidth < mouseOnTarget.X)
                {
                    return new TextPosition(lineIndex, caretLinePartLength);
                }

                int consumedChars = 0;
                float currentWidth = 0;

                foreach (var part in Lines[lineIndex].Parts)
                {
                    for (int i = 0; i < part.VisibleCharLength; i++)
                    {
                        if (currentWidth + part.TextWidth(0, i) + part.TextWidth(i, 1) / 2 > mouseOnTarget.X)
                        {
                            return new TextPosition(lineIndex, consumedChars + i);
                        }
                    }

                    currentWidth += part.TextWidth();
                    consumedChars += part.VisibleCharLength;
                }

                // should not reach this point - already handled in shortcut part
                return new TextPosition(lineIndex, caretLinePartLength);
            }
        }
    }
}
    