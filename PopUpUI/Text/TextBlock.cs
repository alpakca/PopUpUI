using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using PopUpUI.Toolkit;

namespace PopUpUI.Text
{
    /// <summary>Creates a wrapper around <see cref="TextRenderer"/> 
    /// which supports dynamic change of properties of <see cref="TextRenderer"/> to be rendered on same <see cref="RenderTarget2D"/>.</summary>
    public class TextBlock : IDisposable
    {
        private Point textSize;
        private RenderTarget2D textTexture;

        protected bool textBlockChanged = true;
        protected bool sizeMightChanged = true;

        /// <summary>Gets or sets maximum length of each row of text can contain. It must be greater than zero. Default value is <see cref="int.MaxValue"/>.</summary>
        public int HorizontalTextLength
        {
            get => Renderer.HorizontalTextLength;
            set
            {
                if (value <= 0) throw new ArgumentException("HorizontalTextLength must be a positive number.");

                Renderer.HorizontalTextLength = value;

                textBlockChanged = true;
                sizeMightChanged = true;
            }
        }

        /// <summary>Gets or sets the opacity of the text. Expected value is between 0.0f and 1.0f. Default value is 1.0f.</summary>
        public float Opacity
        {
            get => Renderer.Opacity;
            set
            {
                Renderer.Opacity = value;
                textBlockChanged = true;
            }
        }

        /// <summary>Gets or sets the text that will be rendered.</summary>
        public string Text
        {
            get => Renderer.Text;
            set
            {
                Renderer.Text = value;
                TextChanged?.Invoke(this, value);

                textBlockChanged = true;
                sizeMightChanged = true;
            }
        }

        /// <summary>Gets or sets the orientation of the text. Default value is <see cref="TextOrientation.Left"/>.</summary>
        public TextOrientation Orientation
        {
            get => Renderer.Orientation;
            set
            {
                Renderer.Orientation = value;
                textBlockChanged = true;
            }
        }

        ///<summary>Gets or sets the alignment of the text, that is text's position relative to target it will be rendered to.
        /// Default value is <see cref="TextAlignment.TopLeft"/>.</summary>
        public TextAlignment Alignment
        {
            get => Renderer.Alignment;
            set
            {
                Renderer.Alignment = value;
                textBlockChanged = true;
            }
        }

        /// <summary>Gets or sets an offset to be applied to the target text will be rendered to, with <see cref="Alignment"/>.
        /// It also can be used to intentionally creating wiped text.</summary>
        public Point TextOffset
        {
            get => Renderer.TextOffset;
            set
            {
                Renderer.TextOffset = value;
                textBlockChanged = true;
            }
        }

        /// <summary>Gets or sets the color of the text.</summary>
        public virtual Color TextColor
        {
            get => Renderer.TextColor;
            set
            {
                Renderer.TextColor = value;
                textBlockChanged = true;
            }
        }

        /// <summary>Gets or sets a preference for reallocation of <see cref="RenderTarget2D"/> for each text change. Default value is <see langword="false"/>.<br/><br/>
        /// The <see cref="RenderTarget2D"/> associated with this <see cref="TextBlock"/> gets updated:<br/>
        /// Whenever the size of the text get changed, if set to <see langword="true"/>.<br/>
        /// If the new size of the text is bigger than the old one, if set to <see langword="false"/>.
        /// </summary>
        public bool MakeSizeConsistanceWithText { get; set; } = false;

        /// <summary>Indicates the size of <see cref="RenderTarget2D"/> this <see cref="TextBlock"/> contains is constant and won't change as properties are updated.</summary>
        public bool IsFixedSize { get; } = false;
        /// <summary>Gets the size of the target holds the rendered text.</summary>
        public Point Size
        {
            get
            {
                UpdateTextureSizeIfRequired();

                return textSize;
            }
        }
        /// <summary>Gets the font that is used by renderer to create text.</summary>
        public SpriteFont Font => Renderer.Font;
        /// <summary>Gets the renderer this <see cref="TextBlock"/> contains.</summary>
        public TextRenderer Renderer { get; }

        /// <summary>Raised when <see cref="Text"/> has changed.</summary>
        public event Action<TextBlock, string> TextChanged;
        /// <summary>Raised when <see cref="Size"/> has changed.</summary>
        public event Action<TextBlock, Point> TextSizeChanged;

        /// <summary>Initializes a new instance of <see cref="TextBlock"/> class with given <paramref name="renderer"/>.</summary>
        /// <param name="renderer">Renderer to use to dynamically render text.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TextBlock(TextRenderer renderer)
        {
            ArgumentNullException.ThrowIfNull(renderer);

            Renderer = renderer;
        }

        /// <summary>Initializes a new instance of <see cref="TextBlock"/> class with given <paramref name="renderer"/> 
        /// and have a fixed size <see cref="RenderTarget2D"/> in size <paramref name="fixedSize"/>.</summary>
        /// <param name="renderer">Renderer to use to dynamically render text.</param>
        /// <param name="fixedSize">The size of the <see cref="RenderTarget2D"/> which will be constant and won't change as properties are updated.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TextBlock(TextRenderer renderer, Point fixedSize) : this(renderer)
        {
            IsFixedSize = true;

            textSize = fixedSize;
            textTexture = DrawHelper.CreateRenderTarget(fixedSize.X, fixedSize.Y);
        }

        /// <summary>Initializes a new instance of <see cref="TextBlock"/> class with a <see cref="TextRenderer"/> created from <paramref name="font"/>.</summary>
        /// <param name="font">Font to create the renderer that will be used to dynamically render text.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TextBlock(SpriteFont font) : this(new TextRenderer(font))
        {
        }

        /// <summary>Initializes a new instance of <see cref="TextBlock"/> class with a <see cref="TextRenderer"/> created from <paramref name="font"/>
        /// and have a fixed size <see cref="RenderTarget2D"/> in size <paramref name="fixedSize"/>.</summary>
        /// <param name="font">Font to create the renderer that will be used to dynamically render text.</param>
        /// <param name="fixedSize">The size of the <see cref="RenderTarget2D"/> which will be constant and won't change as properties are updated.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TextBlock(SpriteFont font, Point fixedSize) : this(new TextRenderer(font), fixedSize)
        {
        }

        ~TextBlock()
        {
            Dispose();
        }

        /// <summary>Disposes the <see cref="RenderTarget2D"/> this <see cref="TextBlock"/> uses.</summary>
        public void Dispose()
        {
            textTexture?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>Updates the texture of rendered text if any property have changed.</summary>
        /// <returns>Returns the texture of rendered text, as updated if any property was changed.</returns>
        public RenderTarget2D GetTextTexture()
        {
            if (!textBlockChanged) return textTexture;
            UpdateTextureSizeIfRequired();

            Renderer.Render(textTexture);
            textBlockChanged = false;

            return textTexture;
        }

        /// <summary>Indicates if any property has changed from last <see cref="GetTextTexture"/> call.</summary>
        /// <returns><see langword="true"/> if any property was changed without <see cref="GetTextTexture"/> call; otherwise <see langword="false"/>.</returns>
        public bool HasChanged()
        {
            return textBlockChanged;
        }

        private void UpdateTextureSizeIfRequired()
        {
            if (IsFixedSize) return;

            if (sizeMightChanged)
            {
                Point newTextSize = Renderer.GetTextSize();
                if (newTextSize != textSize)
                {
                    if (MakeSizeConsistanceWithText || newTextSize.X > textSize.X || newTextSize.Y > textSize.Y)
                    {
                        textTexture?.Dispose();
                        textTexture = DrawHelper.CreateRenderTarget(newTextSize.X, newTextSize.Y);
                    }

                    textSize = newTextSize;
                    TextSizeChanged?.Invoke(this, newTextSize);
                }
            }
        }
    }
}
