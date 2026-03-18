using Microsoft.Xna.Framework;

namespace PopUpUI.Text
{
    public interface ICaretLayout
    {
        public CaretPosition MinimumPosition { get; }
        public CaretPosition MaximumPosition { get; }

        public CaretPosition Advance(CaretPosition caretPosition, int amount);
        public CaretPosition Clamp(CaretPosition caretPosition);
        public CaretPosition Clamp(CaretPosition caretPosition, int lineIndexOffset, int indexInsideLineOffset);

        public int GetTextCaretPosition(CaretPosition caretPosition);
        public CaretPosition GetCaretPosition(int textCaretPosition);

        public Vector2 ScreenCaretPosition(CaretPosition caretPosition, Point? targetSize);
        public CaretPosition MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize);
    }
}
