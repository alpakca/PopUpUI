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
            return new RichFixedSizeTextProcesserResult(SplitHorizontalLength(), new ProcesserData
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
                    AppendToCurrentLine(node.Value.Text, node.Value.RendererHash);
                    continue;
                }

                int lastStart = 0;
                foreach (var index in newLineIndex)
                {
                    AppendToCurrentLine(node.Value.Text.Substring(lastStart, index - lastStart), node.Value.RendererHash);
                    AddCurrentPieceToLine();

                    hashedWordLines.Add(currentLine.ToArray());

                    currentLine.Clear();
                    lastStart = index + 1;
                }

                AppendToCurrentLine(node.Value.Text.Substring(lastStart), node.Value.RendererHash);
            }

            AddCurrentPieceToLine();
            if (currentLine.Count != 0)
            {
                hashedWordLines.Add(currentLine.ToArray());
            }

            return hashedWordLines;

            void AppendToCurrentLine(string text, IProcesserHash hash)
            {
                int start = 0;
                int end = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == ' ')
                    {
                        if (start != end)
                        {
                            currentPieceParts.Add(text[start..end]);
                            currentPieceHashes.Add(hash);

                            start = end;
                        }

                        start++;
                        AddCurrentPieceToLine();
                    }

                    end++;
                }

                if (start != end)
                {
                    currentPieceParts.Add(text[start..]);
                    currentPieceHashes.Add(hash);
                }
            }

            void AddCurrentPieceToLine()
            {
                if (currentPieceParts.Count == 0)
                {
                    return;
                }

                if (currentPieceParts.Count == 1)
                {
                    currentLine.Add(new SingleHashedTextPiece(currentPieceParts[0], currentPieceHashes[0]));
                }
                else
                {
                    currentLine.Add(new MultiHashedTextPiece(currentPieceParts.ToArray(), currentPieceHashes.ToArray()));
                }

                currentPieceParts.Clear();
                currentPieceHashes.Clear();
            }
        }

        private List<(ITextPiece[] Parts, float TotalWordsWidth)> SplitHorizontalLength()
        {
            List<ITextPiece[]> splitLineWords = SplitNewLinesAndWords();
            List<(ITextPiece[] Parts, float LineWidth)> splitLines = new List<(ITextPiece[] Parts, float LineWidth)>();

            float minimumSpaceWidth = WordSpacing;
            float lineWidth = FirstRowOffset;
            foreach (var lineWords in splitLineWords)
            {
                int lastStart = 0;
                for (int i = 0; i < lineWords.Length; i++)
                {
                    float wordWidth = lineWords[i].TextWidth();

                    if (lineWidth + wordWidth + (i - lastStart) * minimumSpaceWidth <= HorizontalTextLength)
                    {
                        lineWidth += wordWidth;
                    }
                    else
                    {
                        splitLines.Add((lineWords.AsSpan()[lastStart..i].ToArray(), lineWidth));

                        lastStart = i;
                        lineWidth = wordWidth;
                    }
                }

                splitLines.Add((lineWords.AsSpan()[lastStart..].ToArray(), lineWidth));

                lineWidth = 0;
            }

            return splitLines;
        }
    }
}
