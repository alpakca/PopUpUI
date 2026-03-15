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

        private int _visibleCharCount = -1;
        private Vector2 _textSize;

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

            DrawHelper.SetBeginBatch(target, _data.DrawOptions);

            offset += _data.TextOffset.ToVector2() + ParseHelpers.GetAlignmentOffset(_data.Alignment, new Point(target.Width, target.Height), TextSizeFromModified());
            foreach ((string line, float lineWidth) in _lines)
            {
                DrawHelper.Batch.DrawString(_data.Font, 
                                            line, offset + ParseHelpers.GetOrientationOffset(_data.Orientation, _data.HorizontalTextLength, lineWidth), 
                                            _data.TextColor * _data.Opacity);
                offset.Y += _data.LineSpacing;
            }

            DrawHelper.EndBatch();
        }

        public Vector2 GetTextSize()
        {
            if (_textSize == Vector2.Zero)
            {
                _textSize = TextSizeFromModified().ToVector2();
            }

            return _textSize;
        }

        private Point TextSizeFromModified()
        {
            if (_data.HorizontalTextLength != int.MaxValue)
            {
                return new Point(_data.HorizontalTextLength, _data.LineSpacing * _lines.Count);
            }

            return new Point((int)_lines.Max(tuple => tuple.LineWidth), _data.LineSpacing * _lines.Count);
        }

        public int GetVisibleCharCount()
        {
            if (_lines.Count == 0)
            {
                return 0;
            }

            if (_visibleCharCount == -1)
            {
                _visibleCharCount = _lines.Sum(tuple => tuple.Line.Length + 1) - 1;
            }

            return _visibleCharCount;
        }

        public Vector2 ScreenCaretPosition(int caretPosition, Point? targetSize)
        {
            int charConsumed = 0;
            int lineIndex = 0;
            for (; lineIndex < _lines.Count; lineIndex++)
            {
                if (caretPosition <= charConsumed + _lines[lineIndex].Line.Length)
                {
                    break;
                }

                charConsumed += _lines[lineIndex].Line.Length + 1; // +1 for newline char
            }

            if (targetSize.HasValue)
            {
                return new Vector2(ParseHelpers.GetWidth(_data.Font, _lines[lineIndex].Line.Substring(0, caretPosition - charConsumed)), _data.LineSpacing * lineIndex)
                     + _data.TextOffset.ToVector2()
                     + ParseHelpers.GetOrientationOffset(_data.Orientation, _data.HorizontalTextLength, _lines[lineIndex].LineWidth)
                     + ParseHelpers.GetAlignmentOffset(_data.Alignment, targetSize.Value, TextSizeFromModified());
            }

            return new Vector2(ParseHelpers.GetWidth(_data.Font, _lines[lineIndex].Line.Substring(0, caretPosition - charConsumed)), _data.LineSpacing * lineIndex)
                 + _data.TextOffset.ToVector2()
                 + ParseHelpers.GetOrientationOffset(_data.Orientation, _data.HorizontalTextLength, _lines[lineIndex].LineWidth);
        }

        public int MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize)
        {
            if (targetSize.HasValue)
            {
                mouseOnTarget -= ParseHelpers.GetAlignmentOffset(_data.Alignment, targetSize.Value, TextSizeFromModified());
            }

            mouseOnTarget -= _data.TextOffset.ToVector2();

            int lineIndex = (int)(mouseOnTarget.Y / _data.LineSpacing);
            int charsUntilLine = _lines.Take(lineIndex).Sum(tuple => tuple.Line.Length + 1); // +1 for newline chars

            mouseOnTarget -= ParseHelpers.GetOrientationOffset(_data.Orientation, _data.HorizontalTextLength, _lines[lineIndex].LineWidth);

            // shortcut for selecting the last character of line if line width is smaller than mouse position
            if (_lines[lineIndex].LineWidth < mouseOnTarget.X)
            {
                return charsUntilLine + _lines[lineIndex].Line.Length;
            }

            for (int i = 0; i < _lines[lineIndex].Line.Length; i++)
            {
                // also add adjustment for half of the char length
                if (ParseHelpers.GetWidth(_data.Font, _lines[lineIndex].Line.Substring(0, i)) 
                  + ParseHelpers.GetWidth(_data.Font, _lines[lineIndex].Line[i].ToString()) / 2 > mouseOnTarget.X)
                {
                    return charsUntilLine + i;
                }
            }

            // should not reach this point - already handled in shortcut part
            return charsUntilLine + _lines[lineIndex].Line.Length;
        }
    }
}
