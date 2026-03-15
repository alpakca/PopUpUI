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
        private readonly List<(string[] LineWords, float LineWidth)> _lines;

        private int _visibleCharCount = -1;

        internal FixedSizeTextProcesserResult(List<(string[] LineWords, float LineWidth)> lines, ProcesserData data)
        {
            _data = data;
            _lines = lines;
        }

        public void Render(RenderTarget2D target, Vector2 offset)
        {
            DrawHelper.SetBeginBatch(target, _data.DrawOptions);

            offset += _data.TextOffset.ToVector2() + ParseHelpers.GetAlignmentOffset(_data.Alignment, new Point(target.Width, target.Height), TextSizeFromModified());

            foreach ((string[] lineWords, float lineWidth) in _lines)
            {
                float xOffset = 0;
                float wordSpacing = ParseHelpers.GetWordSpacing(_data.HorizontalTextLength, lineWidth, lineWords.Length);

                foreach (var word in lineWords)
                {
                    DrawHelper.Batch.DrawString(_data.Font, word, offset + new Vector2(xOffset, 0), _data.TextColor * _data.Opacity);
                    xOffset += ParseHelpers.GetWidth(_data.Font, word) + wordSpacing;
                }

                offset.Y += _data.LineSpacing;
            }

            DrawHelper.EndBatch();
        }

        private Point TextSizeFromModified()
        {
            return new Point(_data.HorizontalTextLength, _data.LineSpacing * _lines.Count);
        }

        public Vector2 GetTextSize()
        {
            return TextSizeFromModified().ToVector2();
        }

        public int GetVisibleCharCount()
        {
            if (_lines.Count == 0)
            {
                return 0;
            }

            if (_visibleCharCount == -1)
            {
                _visibleCharCount = _lines.Sum(tuple => tuple.LineWords.Sum(word => word.Length + 1)) - 1;
            }

            return _visibleCharCount;
        }

        public Vector2 ScreenCaretPosition(int caretPosition, Point? targetSize)
        {
            if (_lines.Count == 0)
            {
                return _data.TextOffset.ToVector2() 
                     + (targetSize.HasValue ? ParseHelpers.GetAlignmentOffset(_data.Alignment, targetSize.Value, TextSizeFromModified()) : default);
            }

            int charConsumed = 0;

            int lineIndex = 0;
            int wordIndex = -1;
            foreach (var (lineWords, _) in _lines)
            {
                for (int i = 0; i < lineWords.Length; i++)
                {
                    if (caretPosition <= charConsumed + lineWords[i].Length)
                    {
                        wordIndex = i;
                        break;
                    }

                    charConsumed += lineWords[i].Length + 1; // +1 for new line char
                }

                if (wordIndex != -1)
                {
                    break;
                }

                lineIndex++;
            }

            float wordSpacing = ParseHelpers.GetWordSpacing(_data.HorizontalTextLength, _lines[lineIndex].LineWidth, _lines[lineIndex].LineWords.Length);

            // length of words and spaces until the word that caret in
            float xOffset = _lines[lineIndex].LineWords.Take(wordIndex).Sum(word => ParseHelpers.GetWidth(_data.Font, word) + wordSpacing);

            // length of word until caret position
            xOffset += ParseHelpers.GetWidth(_data.Font, _lines[lineIndex].LineWords[wordIndex].Substring(0, caretPosition - charConsumed));

            if (targetSize.HasValue)
            {
                return new Vector2(xOffset, _data.LineSpacing * lineIndex)
                     + _data.TextOffset.ToVector2()
                     + ParseHelpers.GetAlignmentOffset(_data.Alignment, targetSize.Value, TextSizeFromModified());
            }

            return new Vector2(xOffset, _data.LineSpacing * lineIndex)
                 + _data.TextOffset.ToVector2();
        }

        public int MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize)
        {
            if (_lines.Count == 0)
            {
                return 0;
            }

            if (targetSize.HasValue)
            {
                mouseOnTarget -= ParseHelpers.GetAlignmentOffset(_data.Alignment, targetSize.Value, TextSizeFromModified());
            }

            mouseOnTarget -= _data.TextOffset.ToVector2();

            int lineIndex = (int)(mouseOnTarget.Y / _data.LineSpacing);
            float wordSpacing = ParseHelpers.GetWordSpacing(_data.HorizontalTextLength, _lines[lineIndex].LineWidth, _lines[lineIndex].LineWords.Length);

            int charsUntilLine = _lines.Take(lineIndex).Sum(tuple => ParseHelpers.AllWordLengths(tuple.LineWords) + 1); // +1 for newline chars
            int caretLineWordLength = ParseHelpers.AllWordLengths(_lines[lineIndex].LineWords);

            // shortcut for selecting the last character of line if line width is smaller than mouse position
            // LineWidth does NOT contain space character length, add it too for comparison
            if (_lines[lineIndex].LineWidth + wordSpacing * (_lines[lineIndex].LineWords.Length - 1) < mouseOnTarget.X)
            {
                return charsUntilLine + caretLineWordLength;
            }

            int consumedChars = 0;
            float currentWidth = 0;

            foreach (var word in _lines[lineIndex].LineWords)
            {
                for (int i = 0; i < word.Length; i++)
                {
                    if (currentWidth + ParseHelpers.GetWidth(_data.Font, word.Substring(0, i)) + ParseHelpers.GetWidth(_data.Font, word[i].ToString()) / 2 > mouseOnTarget.X)
                    {
                        return charsUntilLine + consumedChars + i;
                    }
                }

                currentWidth += ParseHelpers.GetWidth(_data.Font, word);

                if (currentWidth + wordSpacing / 2 > mouseOnTarget.X)
                {
                    return charsUntilLine + consumedChars + word.Length;
                }

                currentWidth += wordSpacing;
                consumedChars += word.Length + 1;
            }

            // should not reach this point - already handled in shortcut part
            return charsUntilLine + caretLineWordLength;
        }
    }
}
