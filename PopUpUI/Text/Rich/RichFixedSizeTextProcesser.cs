using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace PopUpUI.Text.Rich
{
    /// <summary>Represents a processer that can process text of with <see cref="IProcesserHash"/>es
    /// and processes text with equal space between words. </summary>
    public class RichFixedSizeTextProcesser : RichProcesser
    {
        /// <summary>Initializes a new instance of <see cref="RichFixedSizeTextProcesser"/> with given horizontal text length and default font.</summary>
        /// <param name="horizontalTextLength">The maximum length of each line of to be processed text.</param>
        /// <param name="defaultFont">The font to be used for unhashed text.</param>
        public RichFixedSizeTextProcesser(int horizontalTextLength, SpriteFont defaultFont)
            : base(Color.White, defaultFont)
        {
            ArgumentNullException.ThrowIfNull(defaultFont);

            HorizontalTextLength = horizontalTextLength;
        }

        /// <summary>Initializes a new instance of <see cref="RichFixedSizeTextProcesser"/> with 
        /// copying processer options from given <paramref name="processer"/>.</summary>
        /// <param name="processer">Processer to copy options from.</param>
        public RichFixedSizeTextProcesser(RichFixedSizeTextProcesser processer) : base(processer)
        {
            ArgumentNullException.ThrowIfNull(processer);
        }

        public override ITextProcesserResult GetParseResult()
        {
            return new TextProcesserResult(SplitHorizontalLength(), new ProcesserData
            {
                FirstRowOffset = FirstRowOffset,
                HorizontalTextLength = HorizontalTextLength,
                Alignment = Alignment,
                WordSpacing = WordSpacing,
                LineSpacing = LineSpacing
            });
        }

        private List<ITextPiece[]> SplitNewLinesAndWords()
        {
            LinkedList<SingleHashedTextPiece> hashedNodes = SplitToHashes();

            List<ITextPiece[]> hashedWordLines = new List<ITextPiece[]>();
            List<ITextPiece> currentLine = new List<ITextPiece>();

            List<string> currentPieceParts = new List<string>();
            List<IProcesserHash> currentPieceHashes = new List<IProcesserHash>();

            for (var node = hashedNodes.First; node != null; node = node.Next)
            {
                int[] newLineIndex = ProcesserHelpers.AllNewLineIndexes(node.Value.Text);
                if (newLineIndex.Length == 0)
                {
                    SplitToSpaces(node.Value.Text, node.Value.RendererHash);
                    continue;
                }

                int lastStart = 0;
                foreach (var index in newLineIndex)
                {
                    SplitToSpaces(node.Value.Text.Substring(lastStart, index - lastStart), node.Value.RendererHash);

                    hashedWordLines.Add(currentLine.ToArray());
                    currentLine.Clear();
                    lastStart = index + 1;
                }

                SplitToSpaces(node.Value.Text.Substring(lastStart), node.Value.RendererHash);
            }

            AddCurrentPieceToLine();
            if (currentLine.Count != 0)
            {
                hashedWordLines.Add(currentLine.ToArray());
            }

            return hashedWordLines;

            void SplitToSpaces(string text, IProcesserHash hash)
            {
                string[] words = text.Split(' ');
                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i] == "")
                    {
                        currentLine.Add(new StringTextPiece("", DefaultFont));
                    }
                    else
                    {
                        currentPieceParts.Add(words[i]);
                        currentPieceHashes.Add(hash);

                        AddCurrentPieceToLine();
                    }
                }
            }

            void AddCurrentPieceToLine()
            {
                if (currentPieceParts.Count == 0) return;

                if (currentPieceParts.Count == 1)
                    currentLine.Add(new SingleHashedTextPiece(currentPieceParts[0], currentPieceHashes[0]));

                else
                    currentLine.Add(new MultiHashedTextPiece(currentPieceParts.ToArray(), currentPieceHashes.ToArray()));

                currentPieceParts.Clear();
                currentPieceHashes.Clear();
            }
        }

        private List<SplitLine> SplitHorizontalLength()
        {
            List<ITextPiece[]> splitLineWords = SplitNewLinesAndWords();
            List<SplitLine> splitLines = new List<SplitLine>();

            float minimumSpaceWidth = WordSpacing;
            float lineWidth = FirstRowOffset;
            for (int j = 0; j < splitLineWords.Count; j++)
            {
                int lastStart = 0;
                ITextPiece[] lineWords = splitLineWords[j];
                for (int i = 0; i < lineWords.Length; i++)
                {
                    float wordWidth = lineWords[i].TextWidth();

                    if (lineWidth + wordWidth + (i - lastStart) * minimumSpaceWidth <= HorizontalTextLength)
                    {
                        lineWidth += wordWidth;
                    }
                    else
                    {
                        splitLines.Add(new SplitLine(ProcesserHelpers.ToSpacedPieces(lineWords.AsSpan()[lastStart..i], HorizontalTextLength, lineWidth),
                                       HorizontalTextLength, true)); // count the last space as '\n' char

                        lastStart = i;
                        lineWidth = wordWidth;
                    }
                }

                splitLines.Add(new SplitLine(ProcesserHelpers.ToSpacedPieces(lineWords.AsSpan()[lastStart..], HorizontalTextLength, lineWidth),
                               HorizontalTextLength, j != splitLineWords.Count - 1));

                lineWidth = 0;
            }

            return splitLines;
        }
    }
}
