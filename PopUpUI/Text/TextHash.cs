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
    public class TextHash : IRendererHash
    {
        public bool WrapsText => true;
        public string Symbol { get; }

        public Color Color { get; set; }
        public SpriteFont Font { get; set; }

        public TextHash(string symbol, Color color, SpriteFont font)
        {
            Symbol = symbol;
            Color = color;
            Font = font;
        }

        public void Draw(string text, Vector2 position)
        {
            DrawHelper.Batch.DrawString(Font, text, position, Color);
        }

        public float GetWidth(string str)
        {
            return Font.MeasureString(str).X;
        }

        public int VisibleCharLength(string str)
        {
            return str.Length;
        }
    }
}
