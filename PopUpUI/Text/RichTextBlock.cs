using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

using RenderTarget2D = Microsoft.Xna.Framework.Graphics.RenderTarget2D;

namespace PopUpUI.Text
{
    /// <summary>
    /// Creates a wrapper around <see cref="RichTextRenderer"/> 
    /// which supports dynamic change of properties of <see cref="RichTextRenderer"/> to be rendered on same <see cref="RenderTarget2D"/>.
    /// </summary>
    public class RichTextBlock : TextBlock, IDisposable
    {
        /// <summary>Gets or sets a space length at the first line of text render. Default value is 0.</summary>
        public int SpaceLengthAtBeginning
        {
            get => Renderer.SpaceLengthAtBeginning;
            set
            {
                Renderer.SpaceLengthAtBeginning = value;
                textBlockChanged = true;
            }
        }

        /// <summary>Gets or sets the defualt color of the text which is the part that is not hashed with any <see cref="ColorHash"/> in text.</summary>
        public override Color TextColor 
        {
            get => Renderer.TextColor; 
            set
            {
                Renderer.TextColor = value;
                textBlockChanged = true;
            }
        }

        /// <summary>Gets the color hashes this renderer contains.</summary>
        public IList<ColorHash> ColorHashes => Renderer.ColorHashes;
        /// <summary>Gets the icon hashes this renderer contains.</summary>
        public IList<IconHash> IconHashes => Renderer.IconHashes;

        /// <summary>Gets the renderer this <see cref="RichTextBlock"/> contains.</summary>
        public new RichTextRenderer Renderer { get; }

        /// <summary>Initializes a new instance of <see cref="TextBlock"/> class with given <paramref name="renderer"/>.</summary>
        /// <param name="renderer">Renderer to use to dynamically render text.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RichTextBlock(RichTextRenderer renderer) : base(renderer)
        {
            Renderer = renderer;

            void changeTextBlockChanged() => textBlockChanged = true;
            (ColorHashes as HashCollection<ColorHash>).CollectionChanged += changeTextBlockChanged;
            (IconHashes as HashCollection<IconHash>).CollectionChanged += changeTextBlockChanged;
        }

        /// <summary>Initializes a new instance of <see cref="RichTextBlock"/> class with given <paramref name="renderer"/> 
        /// and have a fixed size <see cref="RenderTarget2D"/> in size <paramref name="fixedSize"/>.</summary>
        /// <param name="renderer">Renderer to use to dynamically render text.</param>
        /// <param name="fixedSize">The size of the <see cref="RenderTarget2D"/> which will be constant and won't change as properties are updated.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RichTextBlock(RichTextRenderer renderer, Point fixedSize) : base(renderer, fixedSize)
        {
            Renderer = renderer;

            void changeTextBlockChanged() => textBlockChanged = true;
            (ColorHashes as HashCollection<ColorHash>).CollectionChanged += changeTextBlockChanged;
            (IconHashes as HashCollection<IconHash>).CollectionChanged += changeTextBlockChanged;
        }

        ~RichTextBlock()
        {
            Dispose();
        }
    }
}
