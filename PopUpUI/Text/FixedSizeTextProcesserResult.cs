using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using System.Collections.Generic;
using System.Linq;

namespace PopUpUI.Text
{
    public class FixedSizeTextProcesserResult : ITextProcesserResult
    {
        private readonly ProcesserData _data;
        private readonly List<(StringTextPiece[] Words, float TotalWordsWidth)> _lines;

        private LineLayout _lineLayout;
        private CaretLayout _caret;

        public ILineLayout Lines => _lineLayout ??= new LineLayout(this);
        public ICaretLayout Caret => _caret ??= new CaretLayout(this);

        public Vector2 TextSize => new Vector2(_data.HorizontalTextLength, _data.LineSpacing * _lines.Count);

        internal FixedSizeTextProcesserResult(List<(StringTextPiece[] Words, float LineWidth)> lines, ProcesserData data)
        {
            _data = data;
            _lines = lines;
        }

        public void Render(RenderTarget2D target, Vector2 offset)
        {
            DrawHelper.SetBeginBatch(target, _data.DrawOptions);

            offset += _data.TextOffset.ToVector2() + ProcesserHelpers.GetAlignmentOffset(_data.Alignment, new Point(target.Width, target.Height), TextSize);

            float xOffset = _data.FirstRowOffset;
            foreach ((StringTextPiece[] words, float lineWidth) in _lines)
            {
                float wordSpacing = ProcesserHelpers.GetWordSpacing(_data.HorizontalTextLength, lineWidth, words.Length);
                foreach (StringTextPiece word in words)
                {
                    word.Draw(offset + new Vector2(xOffset, 0), _data.TextColor, _data.Opacity);
                    xOffset += word.TextWidth() + wordSpacing;
                }

                offset.Y += _data.LineSpacing;
                xOffset = 0;
            }

            DrawHelper.EndBatch();
        }

        private class LineLayout : ILineLayout
        {
            private int _visibleCharCount = -1;
            private readonly FixedSizeTextProcesserResult _result;

            private ProcesserData Data => _result._data;
            private List<(StringTextPiece[] Words, float TotalWordsWidth)> Lines => _result._lines;

            public int LineCount => Lines.Count;
            public int VisibleCharCount => _visibleCharCount == -1 ? _visibleCharCount = GetVisibleCharCount() : _visibleCharCount;

            public LineLayout(FixedSizeTextProcesserResult result)
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

            public float GetLineWidth(TextPosition position)
            {
                if (Lines.Count == 0)
                {
                    return 0;
                }

                return ProcesserHelpers.GetWordsLineWidth(Lines[position.LineIndex].Words,
                                                                ProcesserHelpers.GetWordSpacing(Data.HorizontalTextLength,
                                                                                                Lines[position.LineIndex].TotalWordsWidth,
                                                                                                Lines[position.LineIndex].Words.Length),
                                                                position.IndexInsideLine,
                                                                GetLineVisibleCharCount(position.LineIndex) - position.IndexInsideLine);
            }

            public float GetLineWidth(TextPosition position, int length)
            {
                if (Lines.Count == 0)
                {
                    return 0;
                }

                return ProcesserHelpers.GetWordsLineWidth(Lines[position.LineIndex].Words,
                                                          ProcesserHelpers.GetWordSpacing(Data.HorizontalTextLength,
                                                                                          Lines[position.LineIndex].TotalWordsWidth,
                                                                                          Lines[position.LineIndex].Words.Length),
                                                          position.IndexInsideLine,
                                                          length);
            }
        }

        private class CaretLayout : ICaretLayout
        {
            private readonly FixedSizeTextProcesserResult _result;

            private ProcesserData Data => _result._data;
            private List<(StringTextPiece[] Words, float TotalWordsWidth)> Lines => _result._lines;

            public TextPosition MinimumPosition => new TextPosition(0, 0);
            public TextPosition MaximumPosition => Lines.Count != 0 ? new TextPosition(Lines.Count - 1, _result.Lines.GetLineVisibleCharCount(Lines.Count - 1))
                                                                     : new TextPosition(0, 0);

            public CaretLayout(FixedSizeTextProcesserResult result)
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
                mouseOnTarget -= Data.TextOffset.ToVector2();
                if (targetSize.HasValue)
                {
                    mouseOnTarget -= ProcesserHelpers.GetAlignmentOffset(Data.Alignment, targetSize.Value, _result.TextSize);
                }

                int lineIndex = (int)(mouseOnTarget.Y / Data.LineSpacing);
                float wordSpacing = ProcesserHelpers.GetWordSpacing(Data.HorizontalTextLength, Lines[lineIndex].TotalWordsWidth, Lines[lineIndex].Words.Length);

                int caretLineLength = ProcesserHelpers.WordsVisibleCharCount(Lines[lineIndex].Words);

                // shortcut for selecting the last character of line if line width is smaller than mouse position
                // LineWidth does NOT contain space character length, add it too for comparison
                if (Lines[lineIndex].TotalWordsWidth + wordSpacing * (Lines[lineIndex].Words.Length - 1) < mouseOnTarget.X)
                {
                    return new TextPosition(lineIndex, caretLineLength);
                }

                int consumedChars = 0;
                float currentWidth = lineIndex == 0 ? Data.FirstRowOffset : 0;

                foreach (var word in Lines[lineIndex].Words)
                {
                    for (int i = 0; i < word.VisibleCharLength; i++)
                    {
                        if (currentWidth + word.TextWidth(0, i) + word.TextWidth(i, 1) / 2 > mouseOnTarget.X)
                        {
                            return new TextPosition(lineIndex, consumedChars + i);
                        }
                    }

                    currentWidth += word.TextWidth();

                    if (currentWidth + wordSpacing / 2 > mouseOnTarget.X)
                    {
                        return new TextPosition(lineIndex, consumedChars + word.VisibleCharLength);
                    }

                    currentWidth += wordSpacing;
                    consumedChars += word.VisibleCharLength + 1;
                }

                // should not reach this point - already handled in shortcut part
                return new TextPosition(lineIndex, caretLineLength);
            }
        }
    }
}
