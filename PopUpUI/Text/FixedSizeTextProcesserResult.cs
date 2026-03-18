using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PopUpUI.Text
{
    public class FixedSizeTextProcesserResult : ITextProcesserResult
    {
        private readonly ProcesserData _data;
        private readonly List<(string[] LineWords, float TotalWordsWidth)> _lines;

        private int _visibleCharCount = -1;
        private CaretLayout _caret;

        public Vector2 TextSize => new Vector2(_data.HorizontalTextLength, _data.LineSpacing * _lines.Count);

        public int LineCount => _lines.Count;
        public int VisibleCharCount => _visibleCharCount == -1 ? _visibleCharCount = GetVisibleCharCount() : _visibleCharCount;

        public ICaretLayout Caret => _caret ??= new CaretLayout(this);

        internal FixedSizeTextProcesserResult(List<(string[] LineWords, float LineWidth)> lines, ProcesserData data)
        {
            _data = data;
            _lines = lines;
        }

        public void Render(RenderTarget2D target, Vector2 offset)
        {
            DrawHelper.SetBeginBatch(target, _data.DrawOptions);

            offset += _data.TextOffset.ToVector2() + ProcesserHelpers.GetAlignmentOffset(_data.Alignment, new Point(target.Width, target.Height), TextSize);

            foreach ((string[] lineWords, float lineWidth) in _lines)
            {
                float xOffset = 0;
                float wordSpacing = ProcesserHelpers.GetWordSpacing(_data.HorizontalTextLength, lineWidth, lineWords.Length);

                foreach (var word in lineWords)
                {
                    DrawHelper.Batch.DrawString(_data.Font, word, offset + new Vector2(xOffset, 0), _data.TextColor * _data.Opacity);
                    xOffset += ProcesserHelpers.GetWidth(_data.Font, word) + wordSpacing;
                }

                offset.Y += _data.LineSpacing;
            }

            DrawHelper.EndBatch();
        }

        public int GetLineVisibleCharCount(int lineIndex)
        {
            return ProcesserHelpers.LineVisibleCharCount(_lines[lineIndex].LineWords);
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

            return _lines.Sum(tuple => tuple.LineWords.Sum(word => word.Length + 1)) - 1;
        }

        private class CaretLayout : ICaretLayout
        {
            private readonly FixedSizeTextProcesserResult _result;

            private ProcesserData Data => _result._data;
            private List<(string[] LineWords, float TotalWordsWidth)> Lines => _result._lines;

            public CaretPosition MinimumPosition => new CaretPosition(0, 0);
            public CaretPosition MaximumPosition => new CaretPosition(Lines.Count - 1, _result.GetLineVisibleCharCount(Lines.Count - 1));

            public CaretLayout(FixedSizeTextProcesserResult result)
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
                return new CaretPosition(caretPosition.LineIndex + lineIndexOffset,
                                         caretPosition.IndexInsideLine + indexInsideLineOffset);
            }

            public int GetTextCaretPosition(CaretPosition caretPosition)
            {
                return Lines.Take(caretPosition.LineIndex).Sum(tuple => ProcesserHelpers.LineVisibleCharCount(tuple.LineWords) + 1) // +1 for new line char
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
                                                                    Lines[caretPosition.LineIndex].LineWords.Length);

                // length of words and spaces until the word that caret in
                float xOffset = ProcesserHelpers.WidthUntil(Lines[caretPosition.LineIndex].LineWords, Data.Font, wordSpacing, caretPosition.IndexInsideLine);

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
                float wordSpacing = ProcesserHelpers.GetWordSpacing(Data.HorizontalTextLength, Lines[lineIndex].TotalWordsWidth, Lines[lineIndex].LineWords.Length);

                int caretLineLength = ProcesserHelpers.LineVisibleCharCount(Lines[lineIndex].LineWords);

                // shortcut for selecting the last character of line if line width is smaller than mouse position
                // LineWidth does NOT contain space character length, add it too for comparison
                if (Lines[lineIndex].TotalWordsWidth + wordSpacing * (Lines[lineIndex].LineWords.Length - 1) < mouseOnTarget.X)
                {
                    return new CaretPosition(lineIndex, caretLineLength);
                }

                int consumedChars = 0;
                float currentWidth = 0;

                foreach (var word in Lines[lineIndex].LineWords)
                {
                    for (int i = 0; i < word.Length; i++)
                    {
                        if (currentWidth
                          + ProcesserHelpers.GetWidth(Data.Font, word.Substring(0, i))
                          + ProcesserHelpers.GetWidth(Data.Font, word[i].ToString()) / 2 > mouseOnTarget.X)
                        {
                            return new CaretPosition(lineIndex, consumedChars + i);
                        }
                    }

                    currentWidth += ProcesserHelpers.GetWidth(Data.Font, word);

                    if (currentWidth + wordSpacing / 2 > mouseOnTarget.X)
                    {
                        return new CaretPosition(lineIndex, consumedChars + word.Length);
                    }

                    currentWidth += wordSpacing;
                    consumedChars += word.Length + 1;
                }

                // should not reach this point - already handled in shortcut part
                return new CaretPosition(lineIndex, caretLineLength);
            }
        }
    }
}
