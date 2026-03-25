using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PopUpUI.Text
{
    public interface ITextProcesserResult
    {
        public ILineLayout Lines { get; }
        public ICaretLayout Caret { get; }

        public Vector2 TextSize { get; }
        public void Render(RenderTarget2D target, Vector2 offset);
    }
}
