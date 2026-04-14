using Microsoft.Xna.Framework;

namespace PopUpUI.Text
{
    /// <summary>Provides caret functionality of a <see cref="ITextProcesserResult"/>.</summary>
    public interface ICaretLayout
    {
        /// <summary>Gets a <see cref="TextPosition"/> object presenting the minimum caret position in processed text.</summary>
        public TextPosition MinimumPosition { get; }
        /// <summary>Gets a <see cref="TextPosition"/> object presenting the maximum caret position in processed text.</summary>
        public TextPosition MaximumPosition { get; }

        /// <summary>Advances the given <paramref name="caretPosition"/> by given <paramref name="amount"/> while keeping it inside text.</summary>
        /// <param name="caretPosition">The position of caret.</param>
        /// <param name="amount">The amount of characters to advance caret.</param>
        /// <returns>The new <see cref="TextPosition"/> of caret after advancing.</returns>
        public TextPosition Advance(TextPosition caretPosition, int amount);
        /// <summary>Clamps the given <paramref name="caretPosition"/> to appear inside a valid position in text.</summary>
        /// <param name="caretPosition">The position of caret to clamp.</param>
        /// <returns>The clamped position of caret.</returns>
        public TextPosition Clamp(TextPosition caretPosition);

        /// <summary>Gets the line-free text position of given <paramref name="caretPosition"/>.</summary>
        /// <param name="caretPosition">The position of caret to get line-free position.</param>
        /// <returns>The line-free position of given <paramref name="caretPosition"/>.</returns>
        /// <remarks>Line-free position of a caret position is, an caret index that 
        /// is independent from newline character and counts it as a normal character.</remarks>
        public int GetLineFreePosition(TextPosition caretPosition);
        /// <summary>Gets the text position of given <paramref name="lineFreeCaretPosition"/>.</summary>
        /// <param name="lineFreeCaretPosition">The line-free position of caret to get text position.</param>
        /// <returns>The text position of given <paramref name="lineFreeCaretPosition"/>.</returns>
        /// <remarks>Line-free position of a caret position is, an caret index that 
        /// is independent from newline character and counts it as a normal character.</remarks>
        public TextPosition GetCaretPosition(int lineFreeCaretPosition);

        /// <summary>Gets the screen position of caret to appear at <paramref name="caretPosition"/>, 
        /// in a target that has a size of <paramref name="targetSize"/>.</summary>
        /// <param name="caretPosition">The position of caret to get screen position.</param>
        /// <param name="targetSize">The size of target caret will appear in. 
        /// Used for aligning caret's position with <see cref="TextAlignment"/>.</param>
        /// <returns>The screen position of caret inside target with size of <paramref name="targetSize"/>.</returns>
        public Vector2 ScreenCaretPosition(TextPosition caretPosition, Point? targetSize);
        /// <summary>Gets the text position that caret would appear depending on given mouse position.</summary>
        /// <param name="mouseOnTarget">The mouse's position on target to get text position of caret at that position.</param>
        /// <param name="targetSize">The size of target mouse been clicked on.
        /// Used for aligned-position of caret with <see cref="TextAlignment"/>.</param>
        /// <returns>The text position of a possible caret that would appear at given mouse position.</returns>
        public TextPosition MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize);
    }
}
