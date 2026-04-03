using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace PopUpUI.Text
{
    public abstract class BasicProcesser : ITextProcesser
    {
        public int LineSpacing { get; set; }
        public float WordSpacing { get; set; }

        public int HorizontalTextLength { get; set; } = int.MaxValue;
        public float FirstRowOffset { get; set; } = 0;
        public string Text { get; set; } = "";

        public TextAlignment Alignment { get; set; } = TextAlignment.TopLeft;

        /// <summary>Gets or sets the color of the text. Default value is <see cref="Color.White"/>.</summary>
        public Color TextColor { get; set; } = Color.White;

        /// <summary>Gets the font that is used by renderer to create text.</summary>
        public SpriteFont Font { get; }

        public abstract ITextProcesserResult GetParseResult();

        public BasicProcesser(SpriteFont font)
        {
            ArgumentNullException.ThrowIfNull(font);

            Font = font;
            LineSpacing = font.LineSpacing;
            WordSpacing = font.MeasureString(" ").X;
        }
    }
}
