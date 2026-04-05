using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using System.Collections.Generic;
using System.Linq;

namespace PopUpUI.Text.Rich
{
    public class RichFixedSizeTextProcesserResult : ITextProcesserResult
    {
        private readonly ProcesserData _data;
        private readonly List<(ITextPiece[] Words, float TotalWordsWidth)> _lines;

        private LineLayout _lineLayout;
        private CaretLayout _caret;

        public ILineLayout Lines => _lineLayout ??= new LineLayout(this);
        public ICaretLayout Caret => _caret ??= new CaretLayout(this);

        public Vector2 TextSize => new Vector2(_data.HorizontalTextLength, _data.LineSpacing * _lines.Count);
        public ProcesserData Data => _data;

        internal RichFixedSizeTextProcesserResult(List<(ITextPiece[] Words, float TotalWordsWidth)> lines, ProcesserData data)
        {
            _data = data;
            _lines = lines;
        }

        public void Render(RenderTarget2D target, Vector2 offset, float opacity)
        {
            DrawHelper.SetBeginBatch(target, _data.DrawOptions);

            offset += ProcesserHelpers.GetAlignmentOffset(_data.Alignment, new Point(target.Width, target.Height), TextSize);

            float xOffset = _data.FirstRowOffset;
            foreach ((ITextPiece[] line, float lineWidth) in _lines)
            {
                float wordSpacing = ProcesserHelpers.GetWordSpacing(_data.HorizontalTextLength, lineWidth, line.Length);
                foreach (ITextPiece hashedPiece in line)
                {
                    hashedPiece.Draw(offset + new Vector2(xOffset, 0), default, opacity); // color parameter is not used in rich processers
                    xOffset += hashedPiece.TextWidth() + wordSpacing;
                }

                offset.Y += _data.LineSpacing;
                xOffset = 0;
            }

            DrawHelper.EndBatch();
        }

        private class LineLayout : ILineLayout
        {
            private int _visibleCharCount = -1;
            private readonly RichFixedSizeTextProcesserResult _result;

            private ProcesserData Data => _result._data;
            private List<(ITextPiece[] Words, float TotalWordsWidth)> Lines => _result._lines;

            public int LineCount => Lines.Count;
            public int VisibleCharCount => _visibleCharCount == -1 ? _visibleCharCount = GetVisibleCharCount() : _visibleCharCount;

            public LineLayout(RichFixedSizeTextProcesserResult result)
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
                    count += ProcesserHelpers.WordsVisibleCharCount(Lines[startLineIndex].Words) + 1; // +1 newline char

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

                return ProcesserHelpers.WordsVisibleCharCount(Lines[lineIndex].Words);
            }

            public float GetLineWidth(int lineIndex)
            {
                if (Lines.Count == 0)
                {
                    return 0;
                }

                return Data.HorizontalTextLength;
            }

            public float GetLineWidth(TextPosition startPosition)
            {
                if (Lines.Count == 0)
                {
                    return 0;
                }

                return ProcesserHelpers.GetWordsLineWidth(Lines[startPosition.LineIndex].Words,
                                                          ProcesserHelpers.GetWordSpacing(Data.HorizontalTextLength,
                                                                                          Lines[startPosition.LineIndex].TotalWordsWidth,
                                                                                          Lines[startPosition.LineIndex].Words.Length),
                                                          startPosition.IndexInsideLine,
                                                          GetLineVisibleCharCount(startPosition.LineIndex) - startPosition.IndexInsideLine);
            }

            public float GetLineWidth(TextPosition startPosition, int length)
            {
                if (Lines.Count == 0)
                {
                    return 0;
                }

                return ProcesserHelpers.GetWordsLineWidth(Lines[startPosition.LineIndex].Words,
                                                          ProcesserHelpers.GetWordSpacing(Data.HorizontalTextLength,
                                                                                          Lines[startPosition.LineIndex].TotalWordsWidth,
                                                                                          Lines[startPosition.LineIndex].Words.Length),
                                                          startPosition.IndexInsideLine,
                                                          length);
            }
        }

        private class CaretLayout : ICaretLayout
        {
            private readonly RichFixedSizeTextProcesserResult _result;

            private ProcesserData Data => _result._data;
            private List<(ITextPiece[] Words, float TotalWordsWidth)> Lines => _result._lines;

            public TextPosition MinimumPosition => new TextPosition(0, 0);
            public TextPosition MaximumPosition => Lines.Count != 0 ? new TextPosition(Lines.Count - 1, _result.Lines.GetLineVisibleCharCount(Lines.Count - 1))
                                                                     : new TextPosition(0, 0);

            public CaretLayout(RichFixedSizeTextProcesserResult result)
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
                    return ProcesserHelpers.FirstRowOffsetIfFirst(caretPosition.LineIndex, Data.FirstRowOffset)
                         + (targetSize.HasValue ? ProcesserHelpers.GetAlignmentOffset(Data.Alignment, targetSize.Value, _result.TextSize) : default);
                }

                float wordSpacing = ProcesserHelpers.GetWordSpacing(Data.HorizontalTextLength,
                                                                    Lines[caretPosition.LineIndex].TotalWordsWidth,
                                                                    Lines[caretPosition.LineIndex].Words.Length);

                // length of words and spaces until the word that caret in
                float xOffset = ProcesserHelpers.GetWordsLineWidth(Lines[caretPosition.LineIndex].Words, wordSpacing, 0, caretPosition.IndexInsideLine);

                return ProcesserHelpers.FirstRowOffsetIfFirst(caretPosition.LineIndex, Data.FirstRowOffset)
                     + ProcesserHelpers.GetScreenCaretPosition(caretPosition, Data, _result.TextSize, xOffset, targetSize);
            }

            public TextPosition MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize)
            {
                if (targetSize.HasValue)
                {
                    mouseOnTarget -= ProcesserHelpers.GetAlignmentOffset(Data.Alignment, targetSize.Value, _result.TextSize);
                }

                int lineIndex = (int)(mouseOnTarget.Y / Data.LineSpacing);
                float wordSpacing = ProcesserHelpers.GetWordSpacing(Data.HorizontalTextLength, Lines[lineIndex].TotalWordsWidth, Lines[lineIndex].Words.Length);

                int charsUntilLine = Lines.Take(lineIndex).Sum(tuple => ProcesserHelpers.WordsVisibleCharCount(tuple.Words) + 1); // +1 for newline chars
                int caretLineWordLength = ProcesserHelpers.WordsVisibleCharCount(Lines[lineIndex].Words);

                // shortcut for selecting the last character of line if line width is smaller than mouse position
                if (Lines[lineIndex].TotalWordsWidth + wordSpacing * (Lines[lineIndex].Words.Length - 1) < mouseOnTarget.X)
                {
                    return new TextPosition(lineIndex, caretLineWordLength);
                }

                int consumedChars = 0;
                float currentWidth = lineIndex == 0 ? Data.FirstRowOffset : 0;

                foreach (var part in Lines[lineIndex].Words)
                {
                    for (int i = 0; i < part.VisibleCharLength; i++)
                    {
                        if (currentWidth + part.TextWidth(0, i) + part.TextWidth(i, 1) / 2 > mouseOnTarget.X)
                        {
                            return new TextPosition(lineIndex, consumedChars + i);
                        }
                    }

                    currentWidth += part.TextWidth();

                    if (currentWidth + wordSpacing / 2 > mouseOnTarget.X)
                    {
                        return new TextPosition(lineIndex, consumedChars + part.VisibleCharLength);
                    }

                    currentWidth += wordSpacing;
                    consumedChars += part.VisibleCharLength + 1;
                }

                // should not reach this point - already handled in shortcut
                return new TextPosition(lineIndex, caretLineWordLength);
            }
        }
    }
}
