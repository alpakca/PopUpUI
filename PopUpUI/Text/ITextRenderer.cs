using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PopUpUI.Text
{
    public interface ITextRenderer
    {
        public void Render(RenderTarget2D target, Vector2 offset);
        public Vector2 GetTextSize();

        public Vector2 ScreenCaretPosition(int caretPosition, Point? targetSize);
        public int MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize);
    }
}
