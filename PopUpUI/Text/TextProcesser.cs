using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 8632

namespace PopUpUI.Text
{
    /// <summary>Presents an object that can render text on <see cref="RenderTarget2D"/> in different ways.</summary>
    public class TextProcesser : DefaultProcesser
    {
        private readonly BatchDrawOptions drawOptions;

        /// <summary>Gets or sets the orientation of the text. Default value is <see cref="TextOrientation.Left"/>.</summary>
        public TextOrientation Orientation { get; set; } = TextOrientation.Left;

        /// <summary>Initializes a new instance of <see cref="TextProcesser"/> class with given <paramref name="font"/> and default <see cref="BatchDrawOptions"/>.</summary>
        /// <param name="font"><see cref="SpriteFont"/> that will be used while rendering text.</param>
        public TextProcesser(SpriteFont font) : this(font, null)
        {
        }

        /// <summary>Initializes a new instance of <see cref="TextProcesser"/> class 
        /// with given <paramref name="font"/> and <paramref name="options"/> to use while drawing.</summary>
        /// <param name="font"><see cref="SpriteFont"/> that will be used while rendering text.</param>
        /// <param name="options"><see cref="BatchDrawOptions"/> that will be used while rendering text.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TextProcesser(SpriteFont font, BatchDrawOptions? options) : base(font)
        {
            ArgumentNullException.ThrowIfNull(font, nameof(font));

            drawOptions = options ?? BatchDrawOptions.Default;
        }

        /// <summary>Initializes a new instance of <see cref="TextProcesser"/> class with properties from given <paramref name="processer"/>.</summary>
        /// <param name="processer">A <see cref="TextProcesser"/> instance to copy properties from.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TextProcesser(TextProcesser processer) : base(processer.Font)
        {
            ArgumentNullException.ThrowIfNull(processer, nameof(processer));

            this.Text = processer.Text;
            this.HorizontalTextLength = processer.HorizontalTextLength;
            this.Opacity = processer.Opacity;
            this.Orientation = processer.Orientation;
            this.Alignment = processer.Alignment;
            this.TextOffset = processer.TextOffset;
            this.TextColor = processer.TextColor;
            this.WordSpacing = processer.WordSpacing;
            this.LineSpacing = processer.LineSpacing;
            this.drawOptions = processer.drawOptions;
        }

        public override ITextProcesserResult GetParseResult()
        {
            return new TextProcesserResult(GetModifiedText(), new ProcesserData
            {
                HorizontalTextLength = this.HorizontalTextLength,
                Opacity = this.Opacity,
                Orientation = this.Orientation,
                Alignment = this.Alignment,
                TextOffset = this.TextOffset,
                TextColor = this.TextColor,
                WordSpacing = this.WordSpacing,
                LineSpacing = this.LineSpacing,
                Font = this.Font,
                DrawOptions = this.drawOptions
            });
        }

        public Vector2 GetTextSize()
        {
            float spaceWidth = WordSpacing;

            int lineCount = 1;
            float maxWidth = 0;

            foreach (var existingLine in Text.Split('\n'))
            {
                float lineWidth = 0;
                string[] words = existingLine.Split(' ');

                int lastStart = 0;
                for (int i = 0; i < words.Length; i++)
                {
                    float wordWidth = ProcesserHelpers.GetWidth(Font, words[i]);

                    if (lineWidth + wordWidth + (i - lastStart) * spaceWidth <= HorizontalTextLength)
                    {
                        lineWidth += wordWidth;
                    }
                    else
                    {
                        maxWidth = Math.Max(maxWidth, lineWidth + (i - lastStart) * spaceWidth);

                        lineCount++;
                        lastStart = i;
                        lineWidth = wordWidth;
                    }
                }

                maxWidth = Math.Max(maxWidth, lineWidth + (words.Length - 1 - lastStart) * spaceWidth);
            }

            if (HorizontalTextLength == int.MaxValue)
            {
                return new Vector2(HorizontalTextLength, LineSpacing * lineCount);
            }

            return new Vector2(maxWidth, LineSpacing * lineCount);
        }

        private List<(string Line, float LineWidth)> GetModifiedText()
        {
            float spaceWidth = WordSpacing;
            List<(string Line, float LineWidth)> lines = new List<(string Line, float LineWidth)>();

            foreach (var existingLine in Text.Split('\n'))
            {
                float lineWidth = 0;
                Span<string> words = existingLine.Split(' ');

                int lastStart = 0;
                for (int i = 0; i < words.Length; i++)
                {
                    float wordWidth = ProcesserHelpers.GetWidth(Font, words[i]);

                    if (lineWidth + wordWidth + (i - lastStart) * spaceWidth <= HorizontalTextLength)
                    {
                        lineWidth += wordWidth;
                    }
                    else
                    {
                        lines.Add((string.Join(' ', words.Slice(lastStart, i - lastStart)!), lineWidth + (i - lastStart - 1) * spaceWidth));

                        lastStart = i;
                        lineWidth = wordWidth;
                    }
                }

                lines.Add((string.Join(' ', words.Slice(lastStart)!), lineWidth + (words.Length - 1 - lastStart - 1) * spaceWidth));
            }

            return lines;
        }
    }
}
