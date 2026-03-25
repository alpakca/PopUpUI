using Microsoft.Xna.Framework;

namespace PopUpUI.Text
{
    public interface ICaretLayout
    {
        public TextPosition MinimumPosition { get; }
        public TextPosition MaximumPosition { get; }

        public TextPosition Advance(TextPosition caretPosition, int amount);
        public TextPosition Clamp(TextPosition caretPosition);
        public TextPosition Clamp(TextPosition caretPosition, int lineIndexOffset, int indexInsideLineOffset);

        public int GetTextCaretPosition(TextPosition caretPosition);
        public TextPosition GetCaretPosition(int textCaretPosition);

        public Vector2 ScreenCaretPosition(TextPosition caretPosition, Point? targetSize);
        public TextPosition MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize);
    }
}
