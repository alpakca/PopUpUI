using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;
using System;
using System.Collections.Generic;

#pragma warning disable 8632

namespace PopUpUI.Text
{
    public class FixedSizeTextProcesser : BasicProcesser
    {
        private readonly BatchDrawOptions drawOptions;

        public FixedSizeTextProcesser(int horizontalTextLength, SpriteFont font) : this(horizontalTextLength, font, null)
        {
        }

        public FixedSizeTextProcesser(int horizontalTextLength, SpriteFont font, BatchDrawOptions? options) : base(font)
        {
            ArgumentNullException.ThrowIfNull(font, nameof(font));

            HorizontalTextLength = horizontalTextLength;
            drawOptions = options ?? BatchDrawOptions.Default;
        }

        public FixedSizeTextProcesser(FixedSizeTextProcesser processer) : base(processer.Font)
        {
            this.Text = processer.Text;
            this.FirstRowOffset = processer.FirstRowOffset;
            this.HorizontalTextLength = processer.HorizontalTextLength;
            this.Opacity = processer.Opacity;
            this.Alignment = processer.Alignment;
            this.TextOffset = processer.TextOffset;
            this.TextColor = processer.TextColor;
            this.WordSpacing = processer.WordSpacing;
            this.LineSpacing = processer.LineSpacing;
            this.drawOptions = processer.drawOptions;
        }

        public override ITextProcesserResult GetParseResult()
        {
            return new FixedSizeTextProcesserResult(GetAlignedWords(), new ProcesserData
            {
                FirstRowOffset = this.FirstRowOffset,
                HorizontalTextLength = this.HorizontalTextLength,
                Opacity = this.Opacity,
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
            float minimumSpaceWidth = WordSpacing;
            int lineCount = 1;

            foreach (string existingLine in Text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                float lineWidth = 0;
                Span<string> words = existingLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                int lastStart = 0;
                for (int i = 0; i < words.Length; i++)
                {
                    float wordWidth = ProcesserHelpers.GetWidth(Font, words[i]);

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
        }

        private List<(StringTextPiece[] LineWords, float LineWidth)> GetAlignedWords()
        {
            List<(StringTextPiece[] LineWords, float LineWidth)> lines = new List<(StringTextPiece[] LineWords, float LineWidth)>();

            float minimumSpaceWidth = WordSpacing;

            float lineWidth = FirstRowOffset;
            foreach (string existingLine in Text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                Span<string> words = existingLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                int lastStart = 0;
                for (int i = 0; i < words.Length; i++)
                {
                    float wordWidth = ProcesserHelpers.GetWidth(Font, words[i]);

                    if (lineWidth + wordWidth + (i - lastStart) * minimumSpaceWidth <= HorizontalTextLength)
                    {
                        lineWidth += wordWidth;
                    }
                    else
                    {
                        lines.Add((ProcesserHelpers.ToStringPiece(words[lastStart..i], Font), lineWidth));
                        lastStart = i;

                        lineWidth = wordWidth;
                    }
                }

                lines.Add((ProcesserHelpers.ToStringPiece(words[lastStart..], Font), lineWidth));

                lineWidth = 0;
            }

            return lines;
        }
    }
}
