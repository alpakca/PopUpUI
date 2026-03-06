using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Text;
using PopUpUI.Surface;
using PopUpUI.Toolkit;

#nullable enable

namespace PopUpUI.Text
{
    /// <summary>Presents an object that can render text on <see cref="RenderTarget2D"/> in different ways.</summary>
    public class TextRenderer
    {
        protected readonly BatchDrawOptions drawOptions;

        /// <summary>Gets or sets maximum length of each row of text can contain. Default value is <see cref="int.MaxValue"/>.</summary>
        public int HorizontalTextLength { get; set; } = int.MaxValue;
        /// <summary>Gets or sets the opacity of the text. Expected value is between 0.0f and 1.0f. Default value is 1.0f.</summary>
        public float Opacity { get; set; } = 1;
        /// <summary>Gets or sets the text that will be rendered.</summary>
        public string Text { get; set; } = "";

        /// <summary>Gets or sets the orientation of the text. Default value is <see cref="TextOrientation.Left"/>.</summary>
        public TextOrientation Orientation { get; set; } = TextOrientation.Left;
        /// <summary>Gets or sets the alignment of the text, that is text's position relative to target it will be rendered to.
        /// Default value is <see cref="TextAlignment.TopLeft"/>.</summary>
        /// <remarks>This property does not have any effect if renderer creates a target that is not have a fixed size.</remarks>
        public TextAlignment Alignment { get; set; } = TextAlignment.TopLeft;
        /// <summary>Gets or sets an offset to be applied to the target text will be rendered to, with <see cref="Alignment"/>.</summary>
        /// <remarks>This property does not effect text size from <see cref="GetTextSize"/>. It can be used to intentionally creating wiped text.</remarks>
        public Point TextOffset { get; set; } = new Point();
        /// <summary>Gets or sets the color of the text. Default value is <see cref="Color.White"/>.</summary>
        public virtual Color TextColor { get; set; } = Color.White;

        /// <summary>Gets the font that is used by renderer to create text.</summary>
        public SpriteFont Font { get; }

        /// <summary>Initializes a new instance of <see cref="TextRenderer"/> class with given <paramref name="font"/> and default <see cref="BatchDrawOptions"/>.</summary>
        /// <param name="font"><see cref="SpriteFont"/> that will be used while rendering text.</param>
        public TextRenderer(SpriteFont font) : this(font, null)
        {
        }

        /// <summary>Initializes a new instance of <see cref="TextRenderer"/> class 
        /// with given <paramref name="font"/> and <paramref name="options"/> to use while drawing.</summary>
        /// <param name="font"><see cref="SpriteFont"/> that will be used while rendering text.</param>
        /// <param name="options"><see cref="BatchDrawOptions"/> that will be used while rendering text.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TextRenderer(SpriteFont font, BatchDrawOptions? options)
        {
            ArgumentNullException.ThrowIfNull(font, nameof(font));

            Font = font;
            drawOptions = options ?? BatchDrawOptions.Default;
        }

        /// <summary>Initializes a new instance of <see cref="TextRenderer"/> class with properties from given <paramref name="renderer"/>.</summary>
        /// <param name="renderer">A <see cref="TextRenderer"/> instance to copy properties from.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TextRenderer(TextRenderer renderer)
        {
            ArgumentNullException.ThrowIfNull(renderer, nameof(renderer));

            drawOptions = renderer.drawOptions;

            Text = renderer.Text;
            HorizontalTextLength = renderer.HorizontalTextLength;
            Opacity = renderer.Opacity;
            Orientation = renderer.Orientation;
            Alignment = renderer.Alignment;
            TextOffset = renderer.TextOffset;
            TextColor = renderer.TextColor;

            Font = renderer.Font;
        }

        /// <summary>Creates a new <see cref="RenderTarget2D"/> which has the size of the smallest size the text can fit in
        /// and renders with current properties on created target.</summary>
        /// <returns>Returns the created <see cref="RenderTarget2D"/> that contains rendered text.</returns>
        public RenderTarget2D Render() => Render(GetTextSize());

        /// <summary>Creates a new <see cref="RenderTarget2D"/> which has the size of <paramref name="targetSize"/> 
        /// and renders with current properties on created target.</summary>
        /// <param name="targetSize">The size of <see cref="RenderTarget2D"/> to create.</param>
        /// <returns>The newly created <see cref="RenderTarget2D"/> that contains the render.</returns>
        public RenderTarget2D Render(Point targetSize)
        {
            RenderTarget2D target = DrawHelper.CreateRenderTarget(targetSize.X, targetSize.Y);

            Render(target);
            return target;
        }

        /// <summary>Renders with current properties on <paramref name="target"/>.</summary>
        /// <param name="target"><see cref="RenderTarget2D"/> instance to render text on.</param>
        public virtual void Render(RenderTarget2D target)
        {
            DrawHelper.SetRenderTarget(target);
            DrawHelper.BeginBatch(drawOptions);

            string modifiedText = GetModifiedText();
            Point textSize = GetTextSize();

            float yOffset = GetYOffsetFromAlignment(target.Bounds.Size, textSize);
            switch (Orientation)
            {
                case TextOrientation.Left:
                case TextOrientation.Middle:
                case TextOrientation.Right:

                    foreach (var line in modifiedText.Split('\n'))
                    {
                        float xOffset = GetXOffsetFromOrientation(line) + GetXOffsetFromAlignment(target.Bounds.Size, textSize);
                        DrawHelper.Batch.DrawString(Font, line, new Vector2(xOffset, yOffset) + TextOffset.ToVector2(), TextColor * Opacity);

                        yOffset += Font.LineSpacing;
                    }

                    break;
                case TextOrientation.FixedSize:

                    foreach (var line in modifiedText.Split('\n'))
                    {
                        float xOffset = GetXOffsetFromOrientation(line) + GetXOffsetFromAlignment(target.Bounds.Size, textSize);
                        string[] words = line.Split(' ');
                        float spaceBetweenWords = (HorizontalTextLength - words.Select(GetLineLength).Sum()) / (words.Length - 1);

                        foreach (var word in words)
                        {
                            DrawHelper.Batch.DrawString(Font, word, new Vector2(xOffset, yOffset) + TextOffset.ToVector2(), TextColor * Opacity);
                            xOffset += GetLineLength(word) + spaceBetweenWords;
                        }

                        yOffset += Font.LineSpacing;
                    }

                    break;
            }

            DrawHelper.EndBatch();
        }

        #region RenderWith overloads
        /// <summary>Renders on <paramref name="target"/> by changing <see cref="Text"/> to <paramref name="text"/>.</summary>
        /// <param name="target"><see cref="RenderTarget2D"/> instance to render on.</param>
        /// <param name="text">Text that will be used instead of <see cref="Text"/>.</param>
        public void RenderWith(RenderTarget2D target, string text) 
            => RenderWith(target, text, TextOffset, Alignment);

        /// <summary>Renders on <paramref name="target"/> by changing <see cref="TextOffset"/> to <paramref name="textOffset"/>.</summary>
        /// <param name="target"><see cref="RenderTarget2D"/> instance to render on.</param>
        /// <param name="textOffset">Offset that will used instead of <see cref="TextOffset"/>.</param>
        public void RenderWith(RenderTarget2D target, Point textOffset)
            => RenderWith(target, Text, textOffset, Alignment);

        /// <summary>Renders on <paramref name="target"/> by changing <see cref="Alignment"/> to <paramref name="alignment"/>.</summary>
        /// <param name="target"><see cref="RenderTarget2D"/> instance to render on.</param>
        /// <param name="alignment">Alignment that will be used instead of <see cref="Alignment"/>.</param>
        public void RenderWith(RenderTarget2D target, TextAlignment alignment)
            => RenderWith(target, Text, TextOffset, alignment);

        /// <summary>
        /// Renders on <paramref name="target"/> by changing <see cref="Text"/> and <see cref="TextOffset"/> 
        /// to <paramref name="text"/> and <paramref name="textOffset"/> respectively.
        /// </summary>
        /// <param name="target"><see cref="RenderTarget2D"/> instance to render on.</param>
        /// <param name="text">Text that will be used instead of <see cref="Text"/>.</param>
        /// <param name="textOffset">Offset that will used instead of <see cref="TextOffset"/>.</param>
        public void RenderWith(RenderTarget2D target, string text, Point textOffset) 
            => RenderWith(target, text, textOffset, Alignment);

        /// <summary>
        /// Renders on <paramref name="target"/> by changing <see cref="Text"/> and <see cref="Alignment"/> 
        /// to <paramref name="text"/> and <paramref name="alignment"/> respectively.
        /// </summary>
        /// <param name="target"><see cref="RenderTarget2D"/> instance to render on.</param>
        /// <param name="text">Text that will be used instead of <see cref="Text"/>.</param>
        /// <param name="alignment">Alignment that will be used instead of <see cref="Alignment"/>.</param>
        public void RenderWith(RenderTarget2D target, string text, TextAlignment alignment) 
            => RenderWith(target, text, TextOffset, alignment);

        /// <summary>
        /// Renders on <paramref name="target"/> by changing <see cref="TextOffset"/> and <see cref="Alignment"/> 
        /// to <paramref name="textOffset"/> and <paramref name="alignment"/> respectively.
        /// </summary>
        /// <param name="target"><see cref="RenderTarget2D"/> instance to render on.</param>
        /// <param name="textOffset">Offset that will used instead of <see cref="TextOffset"/>.</param>
        /// <param name="alignment">Alignment that will be used instead of <see cref="Alignment"/>.</param>
        public void RenderWith(RenderTarget2D target, Point textOffset, TextAlignment alignment)
            => RenderWith(target, Text, textOffset, alignment);

        /// <summary>
        /// Renders on <paramref name="target"/> by changing <see cref="Text"/>, <see cref="TextOffset"/> and <see cref="Alignment"/> 
        /// to <paramref name="text"/>, <paramref name="textOffset"/> and <paramref name="alignment"/> respectively.
        /// </summary>
        /// <param name="target"><see cref="RenderTarget2D"/> instance to render on.</param>
        /// <param name="text">Text that will be used instead of <see cref="Text"/>.</param>
        /// <param name="textOffset">Offset that will used instead of <see cref="TextOffset"/>.</param>
        /// <param name="alignment">Alignment that will be used instead of <see cref="Alignment"/>.</param>
        public void RenderWith(RenderTarget2D target, string text, Point textOffset, TextAlignment alignment)
        {
            string oldText = Text;
            Point oldTextOffset = TextOffset;
            TextAlignment oldAlignment = Alignment;

            Text = text;
            TextOffset = textOffset;
            Alignment = alignment;

            Render(target);

            Text = oldText;
            TextOffset = oldTextOffset;
            Alignment = oldAlignment;
        }


        /// <summary>
        /// Renders on a newly created target of size <paramref name="targetSize"/> by changing <see cref="Text"/> to <paramref name="text"/>.
        /// If <paramref name="targetSize"/> is <see langword="null"/>, the smallest size the text can fit is used.
        /// </summary>
        /// <param name="targetSize">The size of target that will be created. If <see langword="null"/>, the smallest size the text can fit is used.</param>
        /// <param name="text">Text that will be used instead of <see cref="Text"/>.</param>
        /// <returns>The newly created <see cref="RenderTarget2D"/> that contains the render.</returns>
        public RenderTarget2D RenderWith(string text, Point? targetSize)
            => RenderWith(text, TextOffset, Alignment, targetSize);

        /// <summary>
        /// Renders on a newly created target of size <paramref name="targetSize"/> by changing <see cref="TextOffset"/> to <paramref name="textOffset"/>.
        /// If <paramref name="targetSize"/> is <see langword="null"/>, the smallest size the text can fit is used.
        /// </summary>
        /// <param name="targetSize">The size of target that will be created. If <see langword="null"/>, the smallest size the text can fit is used.</param>
        /// <param name="textOffset">Offset that will used instead of <see cref="TextOffset"/>.</param>
        /// <returns>The newly created <see cref="RenderTarget2D"/> that contains the render.</returns>
        public RenderTarget2D RenderWith(Point textOffset, Point? targetSize)
            => RenderWith(Text, textOffset, Alignment, targetSize);

        /// <summary>
        /// Renders on a newly created target of size <paramref name="targetSize"/> by changing <see cref="Alignment"/> to <paramref name="alignment"/>.
        /// If <paramref name="targetSize"/> is <see langword="null"/>, the smallest size the text can fit is used.
        /// </summary>
        /// <param name="targetSize">The size of target that will be created. If <see langword="null"/>, the smallest size the text can fit is used.</param>
        /// <param name="alignment">Alignment that will be used instead of <see cref="Alignment"/>.</param>
        /// <returns>The newly created <see cref="RenderTarget2D"/> that contains the render.</returns>
        public RenderTarget2D RenderWith(TextAlignment alignment, Point? targetSize)
            => RenderWith(Text, TextOffset, alignment, targetSize);

        /// <summary>
        /// Renders on a newly created target of size <paramref name="targetSize"/> by changing <see cref="Text"/> and <see cref="TextOffset"/>
        /// to <paramref name="text"/> and <paramref name="textOffset"/> respectively.
        /// If <paramref name="targetSize"/> is <see langword="null"/>, the smallest size the text can fit is used.
        /// </summary>
        /// <param name="targetSize">The size of target that will be created. If <see langword="null"/>, the smallest size the text can fit is used.</param>
        /// <param name="text">Text that will be used instead of <see cref="Text"/>.</param>
        /// <param name="textOffset">Offset that will used instead of <see cref="TextOffset"/>.</param>
        /// <returns>The newly created <see cref="RenderTarget2D"/> that contains the render.</returns>
        public RenderTarget2D RenderWith(string text, Point textOffset, Point? targetSize)
            => RenderWith(text, textOffset, Alignment, targetSize);

        /// <summary>
        /// Renders on a newly created target of size <paramref name="targetSize"/> by changing <see cref="Text"/> and <see cref="Alignment"/> 
        /// to <paramref name="text"/> and <paramref name="alignment"/> respectively.
        /// If <paramref name="targetSize"/> is <see langword="null"/>, the smallest size the text can fit is used.
        /// </summary>
        /// <param name="targetSize">The size of target that will be created. If <see langword="null"/>, the smallest size the text can fit is used.</param>
        /// <param name="text">Text that will be used instead of <see cref="Text"/>.</param>
        /// <param name="alignment">Alignment that will be used instead of <see cref="Alignment"/>.</param>
        /// <returns>The newly created <see cref="RenderTarget2D"/> that contains the render.</returns>
        public RenderTarget2D RenderWith(string text, TextAlignment alignment, Point? targetSize)
            => RenderWith(text, TextOffset, alignment, targetSize);

        /// <summary>
        /// Renders on a newly created target of size <paramref name="targetSize"/> by changing <see cref="TextOffset"/> and <see cref="Alignment"/> 
        /// to <paramref name="textOffset"/> and <paramref name="alignment"/> respectively.
        /// If <paramref name="targetSize"/> is <see langword="null"/>, the smallest size the text can fit is used.
        /// </summary>
        /// <param name="targetSize">The size of target that will be created. If <see langword="null"/>, the smallest size the text can fit is used.</param>
        /// <param name="textOffset">Offset that will used instead of <see cref="TextOffset"/>.</param>
        /// <param name="alignment">Alignment that will be used instead of <see cref="Alignment"/>.</param>
        /// <returns>The newly created <see cref="RenderTarget2D"/> that contains the render.</returns>
        public RenderTarget2D RenderWith(Point textOffset, TextAlignment alignment, Point? targetSize)
            => RenderWith(Text, textOffset, alignment, targetSize);

        /// <summary>
        /// Renders on a newly created target of size <paramref name="targetSize"/> by changing <see cref="Text"/>, <see cref="TextOffset"/> and <see cref="Alignment"/> 
        /// to <paramref name="text"/>, <paramref name="textOffset"/> and <paramref name="alignment"/> respectively.
        /// If <paramref name="targetSize"/> is <see langword="null"/>, the smallest size the text can fit is used.
        /// </summary>
        /// <param name="targetSize">The size of target that will be created. If <see langword="null"/>, the smallest size the text can fit is used.</param>
        /// <param name="text">Text that will be used instead of <see cref="Text"/>.</param>
        /// <param name="textOffset">Offset that will used instead of <see cref="TextOffset"/>.</param>
        /// <param name="alignment">Alignment that will be used instead of <see cref="Alignment"/>.</param>
        /// <returns>The newly created <see cref="RenderTarget2D"/> that contains the render.</returns>
        public RenderTarget2D RenderWith(string text, Point textOffset, TextAlignment alignment, Point? targetSize)
        {
            string oldText = Text;
            Point oldTextOffset = TextOffset;
            TextAlignment oldAlignment = Alignment;

            Text = text;
            TextOffset = textOffset;
            Alignment = alignment;

            RenderTarget2D renderedTexture = Render(targetSize ?? GetTextSize());

            Text = oldText;
            TextOffset = oldTextOffset;
            Alignment = oldAlignment;

            return renderedTexture;
        }
        #endregion

        /// <summary>Calculates the smallest size the text can fit in with current options.</summary>
        /// <returns>Returns the smallest size the text can fit in with current options.</returns>
        public virtual Point GetTextSize()
        {
            string modifiedText = GetModifiedText();

            if (HorizontalTextLength != int.MaxValue)
            {
                return new Point(HorizontalTextLength, (modifiedText.Count(ch => ch == '\n') + 1) * Font.LineSpacing);
            }

            int maxWidth = 0;
            foreach (var line in modifiedText.Split('\n'))
            {
                maxWidth = int.Max(maxWidth, (int)GetLineLength(line));
            }

            return new Point(maxWidth == 0 ? 1 : maxWidth, (modifiedText.AllIndexesOf("\n").Count() + 1) * Font.LineSpacing);
        }

        /// <summary>Calculates where a cursor should be positioned to be at the beginning of index <paramref name="atStartOfIndex"/>.</summary>
        /// <param name="atStartOfIndex">The index position of the cursor.</param>
        /// <param name="targetSizeRenderIn">The size of target that text is rendered for compability with <see cref="TextAlignment"/>. 
        /// Ignores the <see cref="TextAlignment"/> if <see langword="null"/>.</param>
        /// <returns>The position of cursor in <see cref="Vector2"/> inside size of rendered text.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual Vector2 GetCursorPosition(int atStartOfIndex, Point? targetSizeRenderIn)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(atStartOfIndex);

            string modifiedText = GetModifiedText();

            var lines = modifiedText.Split('\n');
            int lineIndexCursorIn = modifiedText.Take(atStartOfIndex).Count(ch => ch == '\n');

            string lineContainingCursor = lines[lineIndexCursorIn];

            float xPosition = GetXOffsetFromOrientation(lineContainingCursor);
            switch (Orientation)
            {
                case TextOrientation.Left:
                case TextOrientation.Middle:
                case TextOrientation.Right:

                    try // this is an escape point when cursor hits the start of the text
                    {
                        xPosition += GetLineLength(lineContainingCursor[..(atStartOfIndex - lines.Take(lineIndexCursorIn).Sum(str => str.Length) - lineIndexCursorIn)]);
                    }
                    catch // if cursor is at the beginning of the text, it is actually downgraded one line; but we increase index to put it in the correct place
                    {
                        lineIndexCursorIn += 1;
                    }

                    break;

                case TextOrientation.FixedSize:

                    string[] words = modifiedText.Split('\n')[lineIndexCursorIn].Split(' ');
                    float spaceLengthBetweenWords
                        = (HorizontalTextLength - words.Select(GetLineLength).Sum()) / (words.Length - 1);

                    foreach (var word in words)
                    {
                        xPosition += GetLineLength(word) + spaceLengthBetweenWords;
                    }

                    xPosition -= spaceLengthBetweenWords;

                    break;
            }

            if (targetSizeRenderIn.HasValue)
            {
                Point textSize = GetTextSize();

                return new Vector2(xPosition, lineIndexCursorIn * Font.LineSpacing) + 
                       TextOffset.ToVector2() +
                       new Vector2(GetXOffsetFromAlignment(targetSizeRenderIn.Value, textSize), GetYOffsetFromAlignment(targetSizeRenderIn.Value, textSize));
            }
            else
            {
                return new Vector2(xPosition, lineIndexCursorIn * Font.LineSpacing) + 
                       TextOffset.ToVector2();
            }
        }

        /// <summary>
        /// Calculates where a cursor should be positioned at the beginning of the index relative to a point inside size of rendered text.</summary>
        /// <remarks>For returning value; if it returns 2, cursor should be positioned between indexes 1 and 2.</remarks>
        /// <param name="positionOnText">A point inside size of rendered text.</param>
        /// <param name="targetSizeRenderIn">The size of target that text is rendered for compability with <see cref="TextAlignment"/>. 
        /// Ignores the <see cref="TextAlignment"/> if <see langword="null"/>.</param>
        /// <returns>The start of index that cursor should be positioned inside size of rendered text.</returns>
        public virtual int GetCursorPosition(Point positionOnText, Point? targetSizeRenderIn)
        {
            Point textSize = GetTextSize();

            positionOnText -= TextOffset;
            if (targetSizeRenderIn.HasValue)
            {
                positionOnText -= new Vector2(GetXOffsetFromAlignment(targetSizeRenderIn.Value, textSize),
                                              GetYOffsetFromAlignment(targetSizeRenderIn.Value, textSize)).ToPoint();
            }

            if (positionOnText.X >= textSize.X || positionOnText.Y >= textSize.Y) return Text.Length;

            string modifiedText = GetModifiedText();

            var lines = modifiedText.Split('\n');
            int lineIndex = positionOnText.Y / Font.LineSpacing;
            string lineCursorIn = lines[lineIndex];

            positionOnText.X -= (int)GetXOffsetFromOrientation(lineCursorIn);

            // just an adjustment to be able to select next position if point already passed half of the character
            int alignAdjustment = Font.LineSpacing / 5;
            for (int i = 0; i <= lineCursorIn.Length; i++)
            {
                if (Font.MeasureString(lineCursorIn[..i]).X - alignAdjustment > positionOnText.X)
                {
                    return i - 1 + lines.Take(lineIndex).Sum(str => str.Length) + lineIndex;
                }
            }

            return lineCursorIn.Length + lines.Take(lineIndex).Sum(str => str.Length) + lineIndex;
        }

        private float GetXOffsetFromOrientation(string line)
        {
            return Orientation switch
            {
                TextOrientation.Left or TextOrientation.FixedSize => 0,
                TextOrientation.Middle => (HorizontalTextLength - GetLineLength(line)) / 2,
                TextOrientation.Right => HorizontalTextLength - GetLineLength(line),
                _ => 0,
            };
        }

        protected float GetXOffsetFromAlignment(Point targetSize, Point textSize)
        {
            return Alignment switch
            {
                TextAlignment.TopLeft or TextAlignment.MiddleLeft or TextAlignment.BottomLeft => 0,
                TextAlignment.TopMiddle or TextAlignment.Center or TextAlignment.BottomMiddle => (targetSize.X - textSize.X) / 2,
                TextAlignment.TopRight or TextAlignment.MiddleRight or TextAlignment.BottomRight => targetSize.X - textSize.X,
                _ => 0
            };
        }

        protected float GetYOffsetFromAlignment(Point targetSize, Point textSize)
        {
            return Alignment switch
            {
                TextAlignment.TopLeft or TextAlignment.TopMiddle or TextAlignment.TopRight => 0,
                TextAlignment.MiddleLeft or TextAlignment.Center or TextAlignment.MiddleRight => (targetSize.Y - textSize.Y) / 2,
                TextAlignment.BottomLeft or TextAlignment.BottomMiddle or TextAlignment.BottomRight => targetSize.Y - textSize.Y,
                _ => 0
            };
        }

        private string GetModifiedText()
        {
            if (Text.Length == 0)
            {
                return "";
            }

            StringBuilder newString = new StringBuilder();
            string[] words = Text.Split(' ');

            StringBuilder line = new StringBuilder();
            float lineLength = 0;
            float spaceLength = Font.MeasureString(" ").X;
            foreach (var word in words)
            {
                float wordLength = GetLineLength(word);

                if (lineLength + wordLength + spaceLength <= HorizontalTextLength)
                {
                    line.Append(word + " ");
                    lineLength += wordLength + spaceLength;
                }
                else if (lineLength + wordLength <= HorizontalTextLength)
                {
                    line.Append(word);
                    lineLength += wordLength;
                }
                else
                {
                    if (line.Length == 0) 
                        throw new ArgumentException("HorizontalTextLength is not enough for the word: " + word + ". It must be bigger than " + wordLength);

                    newString.Append(line.ToString().TrimEnd() + '\n');
                    line.Clear();
                    line.Append(word + " ");
                    lineLength = wordLength + spaceLength;
                }
            }

            return newString.Append(line).ToString();
        }

        private float GetLineLength(string line)
        {
            return Font.MeasureString(line).X;
        }
    }
}
