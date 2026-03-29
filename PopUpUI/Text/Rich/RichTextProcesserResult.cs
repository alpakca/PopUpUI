using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PopUpUI.Text.Rich
{
    public class RichTextProcesserResult : ITextProcesserResult
    {
        private readonly ProcesserData _data;
        private readonly List<(ITextPiece[] Parts, float LineWidth)> _lines;

        private Vector2 _textSize;
        private LineLayout _lineLayout;
        private CaretLayout _caret;

        public ILineLayout Lines => _lineLayout ??= new LineLayout(this);
        public ICaretLayout Caret => _caret ??= new CaretLayout(this);

        public Vector2 TextSize => _textSize == default ? _textSize = GetTextSize() : _textSize;

        internal RichTextProcesserResult(List<(ITextPiece[] Parts, float LineWidth)> lines, ProcesserData data)
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
            foreach ((ITextPiece[] line, float lineWidth) in _lines)
            {
                Vector2 lineOffset = ProcesserHelpers.GetOrientationOffset(_data.Orientation, _data.HorizontalTextLength, lineWidth)
                                   + new Vector2(xOffset, 0);

                foreach (ITextPiece hashedPiece in line)
                {
                    hashedPiece.Draw(offset + lineOffset, default, _data.Opacity); // color parameter is not used in rich processers
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

        private class LineLayout : ILineLayout
        {
            private int _visibleCharCount = -1;
            private readonly RichTextProcesserResult _result;

            private ProcesserData Data => _result._data;
            private List<(ITextPiece[] Parts, float LineWidth)> Lines => _result._lines;

            public int LineCount => Lines.Count;
            public int VisibleCharCount => _visibleCharCount == -1 ? _visibleCharCount = GetVisibleCharCount() : _visibleCharCount;

            public LineLayout(RichTextProcesserResult result)
            {
                _result = result;
            }

            private int GetVisibleCharCount()
            {
                if (Lines.Count == 0)
                {
                    return 0;
                }

                return Lines.Sum(tuple => ProcesserHelpers.LineVisibleCharCount(tuple.Parts) + 1) - 1;
            }

            public int GetLineVisibleCharCount(int lineIndex)
            {
                if (Lines.Count == 0)
                {
                    return 0;
                }

                return ProcesserHelpers.LineVisibleCharCount(Lines[lineIndex].Parts);
            }

            public float GetLineWidth(int lineIndex)
            {
                if (Lines.Count == 0)
                {
                    return 0;
                }

                return Lines[lineIndex].LineWidth;
            }

            public float GetLineWidth(TextPosition position)
            {
                if (Lines.Count == 0)
                {
                    return 0;
                }

                return ProcesserHelpers.GetLineWidth(Lines[position.LineIndex].Parts, 
                                                     Data.WordSpacing, 
                                                     position.IndexInsideLine, 
                                                     GetLineVisibleCharCount(position.LineIndex) - position.IndexInsideLine);
            }

            public float GetLineWidth(TextPosition position, int length)
            {
                if (Lines.Count == 0)
                {
                    return 0;
                }

                return ProcesserHelpers.GetLineWidth(Lines[position.LineIndex].Parts, Data.WordSpacing, position.IndexInsideLine, length);
            }
        }

        private class CaretLayout : ICaretLayout
        {
            private readonly RichTextProcesserResult _result;

            private ProcesserData Data => _result._data;
            private List<(ITextPiece[] Parts, float LineWidth)> Lines => _result._lines;

            public TextPosition MinimumPosition => new TextPosition(0, 0);
            public TextPosition MaximumPosition => Lines.Count != 0 ? new TextPosition(Lines.Count - 1, _result.Lines.GetLineVisibleCharCount(Lines.Count - 1))
                                                                     : new TextPosition(0, 0);

            public CaretLayout(RichTextProcesserResult result)
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
                return Lines.Take(caretPosition.LineIndex).Sum(tuple => ProcesserHelpers.LineVisibleCharCount(tuple.Parts) + 1) // +1 for new line char
                     + caretPosition.IndexInsideLine;
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

                float spaceWidth = Data.WordSpacing;

                // length of words and spaces until the word that caret in
                float xOffset = ProcesserHelpers.GetLineWidth(Lines[caretPosition.LineIndex].Parts, spaceWidth, 0, caretPosition.IndexInsideLine);

                return ProcesserHelpers.FirstRowOffsetIfFirst(caretPosition.LineIndex, Data.FirstRowOffset)
                     + ProcesserHelpers.GetScreenCaretPosition(caretPosition, Data, _result.TextSize, xOffset, targetSize);
            }

            public TextPosition MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize)
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
                    return new TextPosition(lineIndex, caretLineWordLength);
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
                            return new TextPosition(lineIndex, consumedChars + i);
                        }
                    }

                    currentWidth += part.TextWidth();

                    if (currentWidth + spaceWidth / 2 > mouseOnTarget.X)
                    {
                        return new TextPosition(lineIndex, consumedChars + part.VisibleCharLength);
                    }

                    currentWidth += spaceWidth;
                    consumedChars += part.VisibleCharLength + 1;
                }

                // should not reach this point - already handled in shortcut
                return new TextPosition(lineIndex, caretLineWordLength);
            }
        }
    }
}
