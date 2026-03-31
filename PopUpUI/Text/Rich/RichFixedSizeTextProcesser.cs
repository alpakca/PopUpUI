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
            return new RichFixedSizeTextProcesserResult(SplitHorizontalLength(), new ProcesserData
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

        private List<ITextPiece[]> SplitNewLinesAndWords()
        {
            LinkedList<SingleHashedTextPiece> hashedNodes = SplitToHashes();

            List<ITextPiece[]> hashedWordLines = new List<ITextPiece[]>();
            List<ITextPiece> currentLine = new List<ITextPiece>();

            List<string> currentPieceParts = new List<string>();
            List<IRendererHash> currentPieceHashes = new List<IRendererHash>();

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

            void AppendToCurrentLine(string text, IRendererHash hash)
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
