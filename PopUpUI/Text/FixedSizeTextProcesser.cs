using System;
using System.Collections.Generic;

namespace PopUpUI.Text
{
    /// <summary>Represents a basic processer which processes text with equal space between words.</summary>
    public class FixedSizeTextProcesser : BasicProcesser
    {
        /// <summary>Initializes a new instance of <see cref="FixedSizeTextProcesser"/> with given horizontal text length.</summary>
        /// <param name="horizontalTextLength">The maximum length of each line of to be processed text.</param>
        public FixedSizeTextProcesser(int horizontalTextLength)
        {
            HorizontalTextLength = horizontalTextLength;
        }

        /// <summary>Initializes a new instance of <see cref="FixedSizeTextProcesser"/> with 
        /// copying processer options from given <paramref name="processer"/>.</summary>
        /// <param name="processer">Processer to copy options from.</param>
        public FixedSizeTextProcesser(FixedSizeTextProcesser processer) : base(processer)
        {
        }

        public override ITextProcesserResult GetParseResult()
        {
            return new FixedSizeTextProcesserResult(GetAlignedWords(), new ProcesserData
            {
                LineSpacing = this.LineSpacing,
                WordSpacing = this.WordSpacing,
                HorizontalTextLength = this.HorizontalTextLength,
                FirstRowOffset = this.FirstRowOffset,
                Alignment = this.Alignment,
                TextColor = this.TextColor,
                Font = this.Font
            });
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
