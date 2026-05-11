using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopUpUI.Text
{
    /// <summary>Provides information about lines of a <see cref="ITextProcesserResult"/>.</summary>
    public interface ILineLayout
    {
        /// <summary>Gets the number of the lines.</summary>
        public int LineCount { get; }
        /// <summary>Gets the characters that are visible, rendered, to screen. 
        /// It includes the newline characters for each line, except last line.</summary>
        public int VisibleCharCount { get; }

        /// <summary>Gets a value indicating if line at given index was split because of a new line character.</summary>
        /// <param name="lineIndex">The index of line to check split status.</param>
        /// <returns><see langword="true"/> if line was split with a new line character; otherwise, 
        /// if it was split due to <see cref="ITextProcesser.HorizontalTextLength"/>, <see langword="false"/>.</returns>
        public bool SplitWithNewLineChar(int lineIndex);

        /// <summary>Gets the number of visible characters in a line.</summary>
        /// <param name="lineIndex">The line to get the number of visible characters.</param>
        /// <returns>Returns the number of visible characters. It does not include newline character at the end.</returns>
        public int GetLineVisibleCharCount(int lineIndex);
        /// <summary>Gets the total number of visible characters starting from <paramref name="startLineIndex"/> for given count.</summary>
        /// <param name="startLineIndex">The index of the line to start counting.</param>
        /// <param name="lineCount">Number of lines to be included startinf from <paramref name="startLineIndex"/>.</param>
        /// <returns>Returns the number of visible characters starting from <paramref name="startLineIndex"/> for given count. 
        /// It includes newline characters for each end of line, except last one.</returns>
        public int GetVisibleCharCount(int startLineIndex, int lineCount);

        /// <summary>Gets the length of line at <paramref name="lineIndex"/>.</summary>
        /// <param name="lineIndex">The index of the line to get width of.</param>
        /// <returns>Total width of the line at given index.</returns>
        public float GetLineWidth(int lineIndex);
        /// <summary>Gets the length of a line starting at <paramref name="startPosition"/> and until end of the line.</summary>
        /// <param name="startPosition">The position width measurement will start.</param>
        /// <returns>Width of the line starting from <paramref name="startPosition"/> and until the end of the line.</returns>
        public float GetLineWidth(TextPosition startPosition);
        /// <summary>Gets the length of a line starting at <paramref name="startPosition"/> and 
        /// for <paramref name="length"/> of visible characters.</summary>
        /// <param name="startPosition">The position width measurement will start.</param>
        /// <param name="length">The number of visible characters to be measured starting from <paramref name="startPosition"/>.</param>
        /// <returns>Width of the line starting from <paramref name="startPosition"/> and for <paramref name="length"/> of visible characters.</returns>
        public float GetLineWidth(TextPosition startPosition, int length);
    }
}
