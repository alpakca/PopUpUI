using PopUpUI.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace PopUpUI.Text
{
    public class SplitLine
    {
        private readonly int visibleCharCount;
        private readonly ITextPiece[] parts;

        public int VisibleCharCount => visibleCharCount;

        public float LineWidth { get; }
        public bool EndsWithNewLineChar { get; }

        public SplitLine(ITextPiece[] parts, float lineWidth, bool endsWithNewLineChar)
        {
            this.parts = parts;
            LineWidth = lineWidth;
            EndsWithNewLineChar = endsWithNewLineChar;

            for (int i = 0; i < parts.Length; i++)
            {
                visibleCharCount += parts[i].VisibleCharLength;
            }
        }

        public void Deconstruct(out ITextPiece[] parts, out float lineWidth)
        {
            parts = this.parts;
            lineWidth = LineWidth;
        }

        public IEnumerator<ITextPiece> GetEnumerator()
        {
            return ((IEnumerable<ITextPiece>)parts).GetEnumerator();
        }
    }
}
