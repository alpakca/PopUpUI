using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PopUpUI.Text
{
    public interface ITextProcesser
    {
        public int LineSpacing { get; set; }
        public float WordSpacing { get; set; }

        /// <summary>Gets or sets maximum length of each row of text can contain. Default value is <see cref="int.MaxValue"/>.</summary>
        public int HorizontalTextLength { get; set; }
        /// <summary>Gets or sets the opacity of the text. Expected value is between 0.0f and 1.0f. Default value is 1.0f.</summary>
        public float Opacity { get; set; }
        /// <summary>Gets or sets the text that will be parsed.</summary>
        public string Text { get; set; }

        /// <summary>Gets or sets the alignment of the text, that is text's position relative to target it will be rendered to.
        /// Default value is <see cref="TextAlignment.TopLeft"/>.</summary>
        /// <remarks>This property does not have any effect if renderer creates a target that is not have a fixed size.</remarks>
        public TextAlignment Alignment { get; set; }
        /// <summary>Gets or sets an offset to be applied to the target text will be rendered to, addition to use of <see cref="Alignment"/>.</summary>
        /// <remarks>This property does not effect text size from <see cref="GetTextSize"/>. It can be used to intentionally creating wiped text.</remarks>
        public Point TextOffset { get; set; }

        public ITextProcesserResult GetParseResult();
    }
}
