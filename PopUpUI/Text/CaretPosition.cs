using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopUpUI.Text
{
    public readonly struct CaretPosition
    {
        public int LineIndex { get; }
        public int IndexInsideLine { get; }

        public CaretPosition(int lineIndex, int indexInsideLine)
        {
            LineIndex = lineIndex;
            IndexInsideLine = indexInsideLine;
        }
    }
}
