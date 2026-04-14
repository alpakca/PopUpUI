using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace PopUpUI.Text.Rich
{
    /// <summary>Represents a processer that can process text of with <see cref="IProcesserHash"/>es.</summary>
    public class RichTextProcesser : RichProcesser
    {
        /// <summary>Gets or sets the orientation of the text. Default value is <see cref="TextOrientation.Left"/>.</summary>
        public TextOrientation Orientation { get; set; } = TextOrientation.Left;

        /// <summary>Initializes a new instance of <see cref="RichTextProcesser"/> class given defualt font.</summary>
        /// <param name="defaultFont">The font to be used for unhashed text.</param>
        public RichTextProcesser(SpriteFont defaultFont) : base(Color.White, defaultFont)
        {
            ArgumentNullException.ThrowIfNull(defaultFont);
        }

        /// <summary>Initializes a new instance of <see cref="RichTextProcesser"/> with 
        /// copying processer options from given <paramref name="processer"/>.</summary>
        /// <param name="processer">Processer to copy options from.</param>
        public RichTextProcesser(RichTextProcesser processer) : base(processer)
        {
            ArgumentNullException.ThrowIfNull(processer);

            this.Orientation = processer.Orientation;
        }

        public override ITextProcesserResult GetParseResult()
        {
            return new RichTextProcesserResult(SplitHorizontalLength(), new ProcesserData
            {
                FirstRowOffset = this.FirstRowOffset,
                HorizontalTextLength = this.HorizontalTextLength,
                Orientation = this.Orientation,
                Alignment = this.Alignment,
                WordSpacing = this.WordSpacing,
                LineSpacing = this.LineSpacing
            });
        }

        private List<ITextPiece[]> SplitNewLineAndSpaces()
        {
            LinkedList<SingleHashedTextPiece> hashedNodes = SplitToHashes();

            List<ITextPiece[]> hashedLines = new List<ITextPiece[]>();
            List<ITextPiece> currentLine = new List<ITextPiece>();

            List<string> currentPieceParts = new List<string>();
            List<IProcesserHash> currentPieceHashes = new List<IProcesserHash>();

            for (var node = hashedNodes.First; node != null; node = node.Next)
            {
                int[] newLineIndeces = ProcesserHelpers.AllNewLineIndexes(node.Value.Text);
                
                // only split to spaces
                if (newLineIndeces.Length == 0)
                {
                    SplitToSpaces(node.Value.Text, node.Value.RendererHash);
                    continue;
                }

                int lastNewLineCharIndex = 0;
                foreach (var newLineCharIndex in newLineIndeces)
                {
                    SplitToSpaces(node.Value.Text[(lastNewLineCharIndex + 1)..newLineCharIndex], node.Value.RendererHash);

                    hashedLines.Add(currentLine.ToArray());
                    currentLine.Clear();
                    lastNewLineCharIndex = newLineCharIndex;
                }

                SplitToSpaces(node.Value.Text[(lastNewLineCharIndex + 1)..], node.Value.RendererHash);
            }

            AddCurrentPieceToLine();
            if (currentLine.Count != 0)
            {
                hashedLines.Add(currentLine.ToArray());
            }

            return hashedLines;

            void SplitToSpaces(string text, IProcesserHash hash)
            {
                int start = 0;
                int end = 0;
                bool trackingSpace = text[0] == ' '; // texts from SplitToHashes are guaranteed to have length > 0

                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == ' ' && !trackingSpace)
                    {
                        currentPieceParts.Add(text[start..end]);
                        currentPieceHashes.Add(hash);

                        AddCurrentPieceToLine();

                        start = end;
                        trackingSpace = true;
                    }
                    else if (text[i] != ' ' && trackingSpace)
                    {
                        currentLine.Add(new SpaceTextPiece(end - start, WordSpacing));

                        start = end;
                        trackingSpace = false;
                    }

                    end++;
                }

                if (!trackingSpace)
                {
                    currentPieceParts.Add(text[start..]);
                    currentPieceHashes.Add(hash);

                    AddCurrentPieceToLine();
                }
                else
                {
                    currentLine.Add(new SpaceTextPiece(end - start, WordSpacing));
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

        private List<(ITextPiece[] Parts, float LineWidth)> SplitHorizontalLength()
        {
            List<ITextPiece[]> splitLineParts = SplitNewLineAndSpaces();
            List<(ITextPiece[] Parts, float LineWidth)> splitLines = new List<(ITextPiece[] Parts, float LineWidth)>();

            float lineWidth = FirstRowOffset;
            foreach (var lineParts in splitLineParts)
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
