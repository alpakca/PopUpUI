using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopUpUI.Text
{
    /// <summary>Represents a position inside text.</summary>
    public readonly struct TextPosition
    {
        /// <summary>The index of the line.</summary>
        public int LineIndex { get; }
        /// <summary>The index of position inside the line.</summary>
        public int IndexInsideLine { get; }

        /// <summary>Initializes a new instance of <see cref="TextPosition"/> structure
        /// with given <paramref name="lineIndex"/> and <paramref name="indexInsideLine"/>.</summary>
        /// <param name="lineIndex">The index of the line, position belongs to.</param>
        /// <param name="indexInsideLine">The index of character, position refers to.</param>
        public TextPosition(int lineIndex, int indexInsideLine)
        {
            LineIndex = lineIndex;
            IndexInsideLine = indexInsideLine;
        }
    }
}
