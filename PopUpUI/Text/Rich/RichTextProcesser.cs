using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;
using System;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 8632

namespace PopUpUI.Text.Rich
{
    public class RichTextProcesser : RichProcesser
    {
        private readonly BatchDrawOptions drawOptions;

        /// <summary>Gets or sets the orientation of the text. Default value is <see cref="TextOrientation.Left"/>.</summary>
        public TextOrientation Orientation { get; set; } = TextOrientation.Left;

        public RichTextProcesser(SpriteFont defaultFont) : this(defaultFont, null)
        {
        }

        public RichTextProcesser(SpriteFont defaultFont, BatchDrawOptions? options) : base(Color.White, defaultFont)
        {
            ArgumentNullException.ThrowIfNull(defaultFont);

            drawOptions = options ?? BatchDrawOptions.Default;
        }

        public RichTextProcesser(RichTextProcesser processer) : base(processer)
        {
            ArgumentNullException.ThrowIfNull(processer);

            this.Text = processer.Text;
            this.FirstRowOffset = processer.FirstRowOffset;
            this.HorizontalTextLength = processer.HorizontalTextLength;
            this.Opacity = processer.Opacity;
            this.Orientation = processer.Orientation;
            this.Alignment = processer.Alignment;
            this.TextOffset = processer.TextOffset;
            this.WordSpacing = processer.WordSpacing;
            this.LineSpacing = processer.LineSpacing;
            this.drawOptions = processer.drawOptions;
        }

        public override ITextProcesserResult GetParseResult()
        {
            return new RichTextProcesserResult(SplitHorizontalLength(), new ProcesserData
            {
                FirstRowOffset = this.FirstRowOffset,
                HorizontalTextLength = this.HorizontalTextLength,
                Opacity = this.Opacity,
                Orientation = this.Orientation,
                Alignment = this.Alignment,
                TextOffset = this.TextOffset,
                WordSpacing = this.WordSpacing,
                LineSpacing = this.LineSpacing,
                DrawOptions = this.drawOptions,
            });
        }

        private List<ITextPiece[]> SplitNewLineAndSpaces()
        {
            LinkedList<SingleHashedTextPiece> hashedNodes = SplitToHashes();

            List<ITextPiece[]> hashedLines = new List<ITextPiece[]>();
            List<ITextPiece> currentLine = new List<ITextPiece>();

            List<string> currentPieceParts = new List<string>();
            List<IRendererHash> currentPieceHashes = new List<IRendererHash>();

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

            void SplitToSpaces(string text, IRendererHash hash)
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
