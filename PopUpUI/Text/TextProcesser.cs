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
    public class TextProcesser : BasicProcesser
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
            this.FirstRowOffset = processer.FirstRowOffset;
            this.HorizontalTextLength = processer.HorizontalTextLength;
            this.Orientation = processer.Orientation;
            this.Alignment = processer.Alignment;
            this.TextColor = processer.TextColor;
            this.WordSpacing = processer.WordSpacing;
            this.LineSpacing = processer.LineSpacing;
            this.drawOptions = processer.drawOptions;
        }

        public override ITextProcesserResult GetParseResult()
        {
            return new TextProcesserResult(SplitHorizontalLength(), new ProcesserData
            {
                FirstRowOffset = this.FirstRowOffset,
                HorizontalTextLength = this.HorizontalTextLength,
                Orientation = this.Orientation,
                Alignment = this.Alignment,
                TextColor = this.TextColor,
                WordSpacing = this.WordSpacing,
                LineSpacing = this.LineSpacing,
                Font = this.Font,
                DrawOptions = this.drawOptions
            });
        }

        private List<ITextPiece[]> SplitToSpaces()
        {
            List<ITextPiece[]> splitLines = new List<ITextPiece[]>();
            List<ITextPiece> currentLine = new List<ITextPiece>();

            foreach (var existingLine in Text.Split('\n'))
            {
                if (existingLine == string.Empty)
                {
                    splitLines.Add(Array.Empty<ITextPiece>());
                    continue;
                }

                int start = 0;
                int end = 0;
                bool trackingSpace = existingLine[0] == ' ';

                for (int i = 0; i < existingLine.Length; i++)
                {
                    if (existingLine[i] == ' ' && !trackingSpace)
                    {
                        currentLine.Add(new StringTextPiece(existingLine[start..end], Font));

                        start = end;
                        trackingSpace = true;
                    }
                    else if (existingLine[i] != ' ' && trackingSpace)
                    {
                        currentLine.Add(new SpaceTextPiece(end - start, WordSpacing));

                        start = end;
                        trackingSpace = false;
                    }

                    end++;
                }

                if (!trackingSpace)
                {
                    currentLine.Add(new StringTextPiece(existingLine[start..], Font));
                }
                else
                {
                    currentLine.Add(new SpaceTextPiece(end - start, WordSpacing));
                }

                splitLines.Add(currentLine.ToArray());
                currentLine.Clear();
            }

            return splitLines;
        }

        private List<(ITextPiece[] Parts, float LineWidth)> SplitHorizontalLength()
        {
            List<ITextPiece[]> splitSpaceLines = SplitToSpaces();
            List<(ITextPiece[] Parts, float LineWidth)> splitLines = new List<(ITextPiece[] Parts, float LineWidth)>();

            float lineWidth = FirstRowOffset;
            foreach (var lineParts in splitSpaceLines)
            {
                int lastStart = 0;
                for (int i = 0; i < lineParts.Length; i++)
                {
                    float partWidth = lineParts[i].TextWidth();

                    if (lineWidth + partWidth <= HorizontalTextLength)
                    {
                        lineWidth += partWidth;
                    }
                    else
                    {
                        splitLines.Add((lineParts.AsSpan()[lastStart..i].ToArray(), lineWidth));

                        lastStart = i;
                        lineWidth = partWidth;
                    }
                }

                splitLines.Add((lineParts.AsSpan()[lastStart..].ToArray(), lineWidth));

                lineWidth = 0;
            }

            return splitLines;
        }
    }
}
