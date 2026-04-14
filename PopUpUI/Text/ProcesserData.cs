using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PopUpUI.Text
{
    /// <summary>Represents data of a <see cref="ITextProcesser"/>.</summary>
    public class ProcesserData
    {
        /// <summary>Gets the spacing between two lines.</summary>
        public int LineSpacing { get; init; }
        /// <summary>Gets the spacing between two words.</summary>
        public float WordSpacing { get; init; }

        /// <summary>Gets the offset applied for first row.</summary>
        public float FirstRowOffset { get; init; }
        /// <summary>Gets the maximum length of a line.</summary>
        public int HorizontalTextLength { get; init; }

        /// <summary>Gets the alignment of the text.</summary>
        public TextAlignment Alignment { get; init; }
        /// <summary>Gets the orientation of the text.</summary>
        public TextOrientation Orientation { get; init; }

        /// <summary>Gets the color of the text.</summary>
        public Color TextColor { get; init; }
        /// <summary>Gets the font of the text.</summary>
        public SpriteFont Font { get; init; }
    }
}
