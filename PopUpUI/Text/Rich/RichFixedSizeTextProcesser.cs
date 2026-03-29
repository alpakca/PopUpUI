using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;
using System;
using System.Collections.Generic;

#pragma warning disable 8632

namespace PopUpUI.Text.Rich
{
    public class RichFixedSizeTextProcesser : RichProcesser
    {
        private readonly BatchDrawOptions drawOptions;

        public RichFixedSizeTextProcesser(int horizontalTextLength, SpriteFont defaultFont) : this(horizontalTextLength, defaultFont, null)
        {
        }

        public RichFixedSizeTextProcesser(int horizontalTextLength, SpriteFont defaultFont, BatchDrawOptions options)
            : base(Color.White, defaultFont)
        {
            ArgumentNullException.ThrowIfNull(defaultFont);

            HorizontalTextLength = horizontalTextLength;
            drawOptions = options ?? BatchDrawOptions.Default;
        }

        public RichFixedSizeTextProcesser(RichFixedSizeTextProcesser processer) : base(processer.defaultTextHash.Color, processer.defaultTextHash.Font)
        {
            ArgumentNullException.ThrowIfNull(processer);

            Text = processer.Text;
            FirstRowOffset = processer.FirstRowOffset;
            HorizontalTextLength = processer.HorizontalTextLength;
            Opacity = processer.Opacity;
            Alignment = processer.Alignment;
            TextOffset = processer.TextOffset;
            WordSpacing = processer.WordSpacing;
            LineSpacing = processer.LineSpacing;
            drawOptions = processer.drawOptions;

            AttachHashes(processer._hashes);
        }

        public override ITextProcesserResult GetParseResult()
        {
            return new RichFixedSizeTextProcesserResult(GetHashedLines(), new ProcesserData
            {
                FirstRowOffset = FirstRowOffset,
                HorizontalTextLength = HorizontalTextLength,
                Opacity = Opacity,
                Alignment = Alignment,
                TextOffset = TextOffset,
                WordSpacing = WordSpacing,
                LineSpacing = LineSpacing,
                DrawOptions = drawOptions
            });
        }

        private List<(ITextPiece[] Parts, float TotalWordsWidth)> GetHashedLines()
        {
            return SplitHorizontalLength(SplitToNewLines());
        }

        private List<(ITextPiece[] Parts, float TotalWordsWidth)> SplitHorizontalLength(List<ITextPiece[]> lineParts)
        {
            List<ITextPiece> currentLine = new List<ITextPiece>();
            List<(ITextPiece[] Parts, float LineWidth)> lines = new List<(ITextPiece[] Parts, float LineWidth)>();

            float minimumSpaceWidth = WordSpacing;
            float lineWidth = FirstRowOffset;
            foreach (var line in lineParts)
            {
                int lastStart = 0;

                for (int i = 0; i < line.Length; i++)
                {
                    float wordWidth = line[i].TextWidth();

                    if (lineWidth + wordWidth + (i - lastStart) * minimumSpaceWidth <= HorizontalTextLength)
                    {
                        currentLine.Add(line[i]);
                        lineWidth += wordWidth;
                    }
                    else
                    {
                        lines.Add((currentLine.ToArray(), lineWidth));
                        currentLine.Clear();

                        lastStart = i;
                        lineWidth = wordWidth;
                        currentLine.Add(line[i]);
                    }
                }

                lines.Add((currentLine.ToArray(), lineWidth));

                currentLine.Clear();
                lineWidth = 0;
            }

            return lines;
        }
        /*
        public Vector2 GetTextSize()
        {
            List<(string Word, IRendererHash Hash)[]> existingLines = SplitToNewLines(SplitToHashes());

            float minimumSpaceWidth = WordSpacing;
            int lineCount = 1;

            foreach (var line in existingLines)
            {
                float lineWidth = 0;
                int lastStart = 0;

                for (int i = 0; i < line.Length; i++)
                {
                    (string word, IRendererHash hash) = line[i];

                    float wordWidth = hash.GetWidth(word);

                    if (lineWidth + wordWidth + (i - lastStart) * minimumSpaceWidth <= HorizontalTextLength)
                    {
                        lineWidth += wordWidth;
                    }
                    else
                    {
                        lineCount++;
                        lastStart = i;
                        lineWidth = wordWidth;
                    }
                }
            }

            return new Vector2(HorizontalTextLength, LineSpacing * lineCount);
        }*/
    }
}
