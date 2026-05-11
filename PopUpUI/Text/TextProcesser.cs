using System;
using System.Collections.Generic;

namespace PopUpUI.Text
{
    /// <summary>Represents a basic processer that renders and parses a text with different options.</summary>
    public class TextProcesser : BasicProcesser
    {
        /// <summary>Gets or sets the orientation of the text. Default value is <see cref="TextOrientation.Left"/>.</summary>
        public TextOrientation Orientation { get; set; } = TextOrientation.Left;

        /// <summary>Initializes a new instance of <see cref="TextProcesser"/> class.</summary>
        public TextProcesser()
        {
        }

        /// <summary>Initializes a new instance of <see cref="TextProcesser"/> class with properties from given <paramref name="processer"/>.</summary>
        /// <param name="processer">A <see cref="TextProcesser"/> instance to copy properties from.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TextProcesser(TextProcesser processer) : base(processer)
        {
            ArgumentNullException.ThrowIfNull(processer, nameof(processer));

            this.Orientation = processer.Orientation;
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
                Font = this.Font
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

        private List<SplitLine> SplitHorizontalLength()
        {
            List<ITextPiece[]> splitSpaceLines = SplitToSpaces();
            List<SplitLine> splitLines = new List<SplitLine>();

            float lineWidth = FirstRowOffset;
            for (int j = 0; j < splitSpaceLines.Count; j++)
            {
                int lastStart = 0;
                ITextPiece[] lineParts = splitSpaceLines[j];

                for (int i = 0; i < lineParts.Length; i++)
                {
                    float partWidth = lineParts[i].TextWidth();

                    if (lineWidth + partWidth <= HorizontalTextLength)
                    {
                        lineWidth += partWidth;
                    }
                    else
                    {
                        splitLines.Add(new SplitLine(lineParts.AsSpan()[lastStart..i].ToArray(), lineWidth, false));

                        lastStart = i;
                        lineWidth = partWidth;
                    }
                }

                splitLines.Add(new SplitLine(lineParts.AsSpan()[lastStart..].ToArray(), lineWidth, j != splitSpaceLines.Count - 1));

                lineWidth = 0;
            }

            return splitLines;
        }
    }
}
