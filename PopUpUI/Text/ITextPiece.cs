using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopUpUI.Text
{
    public interface ITextPiece
    {
        public int VisibleCharLength { get; }

        public float TextWidth();
        public float TextWidth(int startIndex);
        public float TextWidth(int startIndex, int length);
    }
}
