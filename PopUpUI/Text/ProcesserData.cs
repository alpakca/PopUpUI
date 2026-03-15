using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;

namespace PopUpUI.Text
{
    internal class ProcesserData
    {
        public int LineSpacing { get; init; }
        public float WordSpacing { get; init; }

        /// <summary>Gets or sets maximum length of each row of text can contain. Default value is <see cref="int.MaxValue"/>.</summary>
        public int HorizontalTextLength { get; init; }
        /// <summary>Gets or sets the opacity of the text. Expected value is between 0.0f and 1.0f. Default value is 1.0f.</summary>
        public float Opacity { get; init; }

        /// <summary>Gets or sets the alignment of the text, that is text's position relative to target it will be rendered to.
        /// Default value is <see cref="TextAlignment.TopLeft"/>.</summary>
        /// <remarks>This property does not have any effect if renderer creates a target that is not have a fixed size.</remarks>
        public TextAlignment Alignment { get; init; }
        /// <summary>Gets or sets the orientation of the text. Default value is <see cref="TextOrientation.Left"/>.</summary>
        public TextOrientation Orientation { get; init; }
        /// <summary>Gets or sets an offset to be applied to the target text will be rendered to, addition to use of <see cref="Alignment"/>.</summary>
        /// <remarks>This property does not effect text size from <see cref="GetTextSize"/>. It can be used to intentionally creating wiped text.</remarks>
        public Point TextOffset { get; init; }

        /// <summary>Gets or sets the color of the text. Default value is <see cref="Color.White"/>.</summary>
        public Color TextColor { get; init; } = Color.White;

        public SpriteFont Font { get; init; }

        public BatchDrawOptions DrawOptions { get; init; }
    }
}
