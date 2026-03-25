using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopUpUI.Text
{
    public interface ILineLayout
    {
        public int LineCount { get; }
        public int VisibleCharCount { get; } // also contains new line chars at the end of the lines

        public int GetLineVisibleCharCount(int lineIndex);
        public float GetLineWidth(int lineIndex);
        public float GetLineWidth(TextPosition position);
        public float GetLineWidth(TextPosition position, int length);
    }
}
