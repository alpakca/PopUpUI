using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PopUpUI.Text
{
    /// <summary>Represents a basic processer, that processes text as it is.</summary>
    public abstract class BasicProcesser : ITextProcesser
    {
        public int LineSpacing { get; set; } = -1;
        public float WordSpacing { get; set; } = -1;

        public int HorizontalTextLength { get; set; } = int.MaxValue;
        public float FirstRowOffset { get; set; } = 0;
        public string Text { get; set; } = "";
        public TextAlignment Alignment { get; set; } = TextAlignment.TopLeft;

        /// <summary>Gets or sets the color of the processed text. Default value is <see cref="Color.White"/>.</summary>
        public Color TextColor { get; set; } = Color.White;
        /// <summary>Gets or sets the font of the processed text.</summary>
        public SpriteFont Font
        {
            get;
            set
            {
                LineSpacing = LineSpacing == -1 ? value.LineSpacing : LineSpacing;
                WordSpacing = WordSpacing == -1 ? value.MeasureString(" ").X : WordSpacing;

                field = value;
            }
        }

        public abstract ITextProcesserResult GetParseResult();

        public BasicProcesser()
        {
        }

        public BasicProcesser(BasicProcesser processer)
        {
            LineSpacing = processer.LineSpacing;
            WordSpacing = processer.WordSpacing;
            HorizontalTextLength = processer.HorizontalTextLength;
            FirstRowOffset = processer.FirstRowOffset;
            Text = processer.Text;
            Alignment = processer.Alignment;

            TextColor = processer.TextColor;
            Font = processer.Font;
        }
    }
}
