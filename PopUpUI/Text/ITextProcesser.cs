using Microsoft.Xna.Framework;

namespace PopUpUI.Text
{
    /// <summary>Represents a processer that can parse and provide information about a text.</summary>
    public interface ITextProcesser
    {
        /// <summary>Gets or sets the spacing between each line of text.</summary>
        public int LineSpacing { get; set; }
        /// <summary>Gets or sets the spacing between each word of text.</summary>
        public float WordSpacing { get; set; }
        
        /// <summary>Gets or sets the maximum length of each line.</summary>
        public int HorizontalTextLength { get; set; }
        /// <summary>Gets or sets an offset for the first line of the text.</summary>
        public float FirstRowOffset { get; set; }
        /// <summary>Gets or sets the text that will be processed.</summary>
        public string Text { get; set; }

        /// <summary>Gets or sets the alignment of the text, that is text's position relative to target it will be rendered to.</summary>
        public TextAlignment Alignment { get; set; }

        /// <summary>Parses the <see cref="Text"/> with properties of this processer.</summary>
        /// <returns>The result of parse that provides information about the parsed text.</returns>
        public ITextProcesserResult GetParseResult();
    }
}
