using Microsoft.Xna.Framework;

namespace PopUpUI.Text
{
    public interface IRendererHash
    {
        public bool WrapsText { get; }
        public string Symbol { get; }

        public float GetWidth(string str);
        public int VisibleCharLength(string str);
        public void Draw(string text, Vector2 position);
    }
}
