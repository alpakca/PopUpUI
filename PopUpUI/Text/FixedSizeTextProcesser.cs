using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;
using System;
using System.Collections.Generic;

namespace PopUpUI.Text
{
    public class FixedSizeTextProcesser : DefaultLayoutProcesser
    {
        private readonly BatchDrawOptions drawOptions;

        public FixedSizeTextProcesser(int horizontalTextLength, SpriteFont font) : this(horizontalTextLength, font, null)
        {
        }

        public FixedSizeTextProcesser(int horizontalTextLength, SpriteFont font, BatchDrawOptions options) : base(font)
        {
            ArgumentNullException.ThrowIfNull(font, nameof(font));

            HorizontalTextLength = horizontalTextLength;
            drawOptions = options ?? BatchDrawOptions.Default;
        }

        public override ITextProcesserResult GetParseResult()
        {
            return new FixedSizeTextProcesserResult(GetAlignedWords(), new ProcesserData
            {
                HorizontalTextLength = HorizontalTextLength,
                Opacity = Opacity,
                Alignment = Alignment,
                TextOffset = TextOffset,
                TextColor = TextColor,
                LineSpacing = LineSpacing,
                WordSpacing = WordSpacing,
                Font = Font,
                DrawOptions = drawOptions
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
                    float wordWidth = ParseHelpers.GetWidth(Font, words[i]);

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

        private List<(string[] LineWords, float LineWidth)> GetAlignedWords()
        {
            List<(string[] LineWords, float LineWidth)> lines = new List<(string[] LineWords, float LineWidth)>();

            float minimumSpaceWidth = WordSpacing;

            foreach (string existingLine in Text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                float lineWidth = 0;
                Span<string> words = existingLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                int lastStart = 0;
                for (int i = 0; i < words.Length; i++)
                {
                    float wordWidth = ParseHelpers.GetWidth(Font, words[i]);

                    if (lineWidth + wordWidth + (i - lastStart) * minimumSpaceWidth <= HorizontalTextLength)
                    {
                        lineWidth += wordWidth;
                    }
                    else
                    {
                        lines.Add((words[lastStart..i].ToArray(), lineWidth));
                        lastStart = i;

                        lineWidth = wordWidth;
                    }
                }

                lines.Add((words[lastStart..].ToArray(), lineWidth));
            }

            return lines;
        }
    }
}
