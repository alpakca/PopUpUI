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
            return new TextProcesserResult(GetAlignedWords(), new ProcesserData
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

        private List<SplitLine> GetAlignedWords()
        {
            List<SplitLine> lines = new List<SplitLine>();

            float minimumSpaceWidth = WordSpacing;
            float lineWidth = FirstRowOffset;

            string[] existingLines = Text.Split('\n');
            for (int j = 0; j < existingLines.Length; j++)
            {
                Span<string> words = existingLines[j].Split(' ');

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
                        lines.Add(new SplitLine(ProcesserHelpers.ToSpacedPieces(words[lastStart..i], Font, HorizontalTextLength, lineWidth), 
                                                HorizontalTextLength, true)); // count the last space as '\n' char

                        lastStart = i;
                        lineWidth = wordWidth;
                    }
                }

                lines.Add(new SplitLine(ProcesserHelpers.ToSpacedPieces(words[lastStart..], Font, HorizontalTextLength, lineWidth), 
                                        HorizontalTextLength, j != existingLines.Length - 1));

                lineWidth = 0;
            }

            return lines;
        }
    }
}
