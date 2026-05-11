using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;

namespace PopUpUI.Text
{
    /// <summary>Represents a parse result of <see cref="ITextProcesser"/>.</summary>
    public interface ITextProcesserResult
    {
        /// <summary>Gets the data used in processing text.</summary>
        public ProcesserData Data { get; }
        /// <summary>Gets an <see cref="ILineLayout"/> object that contains information and methods about lines of parsed text.</summary>
        public ILineLayout Lines { get; }
        /// <summary>Gets an <see cref="ICaretLayout"/> object that contains information and methods about caret positions of parsed text.</summary>
        public ICaretLayout Caret { get; }

        /// <summary>Gets the size of the parsed text.</summary>
        public Vector2 TextSize { get; }
        /// <summary>Renders the parsed text using given arguments to an already set <see cref="RenderTarget2D"/>.</summary>
        /// <param name="targetSize">The size of the target text will be rendered to.</param>
        /// <param name="drawOptions">The <see cref="BatchDrawOptions"/> to use while drawing.</param>
        /// <param name="offset">The offset that will be applied to text.</param>
        /// <param name="opacity">The opacity of the text.</param>
        public void Render(BatchDrawOptions drawOptions, Vector2 offset, Point? targetSize, float opacity);
        /// <summary>Renders the parsed text using given arguments to <paramref name="target"/>.</summary>
        /// <param name="target">The target surface text will be rendered to.</param>
        /// <param name="drawOptions">The <see cref="BatchDrawOptions"/> to use while drawing.</param>
        /// <param name="offset">The offset that will be applied to text.</param>
        /// <param name="opacity">The opacity of the text.</param>
        public void Render(RenderTarget2D target, BatchDrawOptions drawOptions, Vector2 offset, float opacity);
    }
}
