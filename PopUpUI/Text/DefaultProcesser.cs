using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PopUpUI.Text
{
    public abstract class DefaultProcesser : ITextProcesser
    {
        private int lineSpacing = -1;
        private float wordSpacing = -1;

        public int LineSpacing
        {
            get => lineSpacing >= 0 ? lineSpacing : Font.LineSpacing;
            set => lineSpacing = value;
        }

        public float WordSpacing
        {
            get => wordSpacing >= 0 ? wordSpacing : Font.MeasureString(" ").X;
            set => wordSpacing = value;
        }

        public int HorizontalTextLength { get; set; } = int.MaxValue;
        public float FirstRowOffset { get; set; } = 0;
        public float Opacity { get; set; } = 1;
        public string Text { get; set; } = "";

        public TextAlignment Alignment { get; set; } = TextAlignment.TopLeft;
        public Point TextOffset { get; set; } = new Point();

        /// <summary>Gets or sets the color of the text. Default value is <see cref="Color.White"/>.</summary>
        public Color TextColor { get; set; } = Color.White;

        /// <summary>Gets the font that is used by renderer to create text.</summary>
        public SpriteFont Font { get; }

        public abstract ITextProcesserResult GetParseResult();

        public DefaultProcesser(SpriteFont font)
        {
            Font = font;
        }
    }
}
