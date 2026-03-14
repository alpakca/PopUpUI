using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#pragma warning disable 8524

namespace PopUpUI.Text
{
    public abstract class DefaultRenderer : ITextRenderer
    {
        private int lineSpacing = -1;
        private float wordSpacing = -1;

        public int LineSpacing
        {
            get => lineSpacing >= 0 ? lineSpacing : Font.LineSpacing;
            set => lineSpacing = value;
        }

        public float WordSpacing
        {
            get => wordSpacing >= 0 ? wordSpacing : Font.MeasureString(" ").X;
            set => wordSpacing = value;
        }

        public int HorizontalTextLength { get; set; } = int.MaxValue;
        public float Opacity { get; set; } = 1;
        public string Text { get; set; } = "";

        public TextAlignment Alignment { get; set; } = TextAlignment.TopLeft;
        public Point TextOffset { get; set; } = new Point();

        /// <summary>Gets or sets the color of the text. Default value is <see cref="Color.White"/>.</summary>
        public Color TextColor { get; set; } = Color.White;

        /// <summary>Gets the font that is used by renderer to create text.</summary>
        public SpriteFont Font { get; }

        public abstract void Render(RenderTarget2D target, Vector2 offset);

        public abstract Vector2 GetTextSize();
        public abstract int GetVisibleCharCount();

        public abstract Vector2 ScreenCaretPosition(int caretPosition, Point? targetSize);
        public abstract int MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize);

        public DefaultRenderer(SpriteFont font)
        {
            Font = font;
        }

        protected float GetWidth(string str)
        {
            return Font.MeasureString(str).X;
        }

        protected Vector2 GetAlignmentOffset(Point surfaceSize, Point textSize)
        {
            int xOffset = Alignment switch
            {
                TextAlignment.TopLeft or TextAlignment.MiddleLeft or TextAlignment.BottomLeft => 0,
                TextAlignment.TopMiddle or TextAlignment.Center or TextAlignment.BottomMiddle => (surfaceSize.X - textSize.X) / 2,
                TextAlignment.TopRight or TextAlignment.MiddleRight or TextAlignment.BottomRight => surfaceSize.X - textSize.X
            };

            int yOffset = Alignment switch
            {
                TextAlignment.TopLeft or TextAlignment.TopMiddle or TextAlignment.TopRight => 0,
                TextAlignment.MiddleLeft or TextAlignment.Center or TextAlignment.MiddleRight => (surfaceSize.Y - textSize.Y) / 2,
                TextAlignment.BottomLeft or TextAlignment.BottomMiddle or TextAlignment.BottomRight => surfaceSize.Y - textSize.Y
            };

            return new Vector2(xOffset, yOffset);
        }
    }
}
