using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;
using System;
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

        public RichTextProcesser(RichTextProcesser processer) : base(processer.defaultTextHash.Color, processer.defaultTextHash.Font)
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

            AttachHashes(processer._hashes);
        }

        public override ITextProcesserResult GetParseResult()
        {
            return new RichTextProcesserResult(GetHashedLines(), new ProcesserData
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

        private List<(ITextPiece[] Parts, float LineWidth)> GetHashedLines()
        {
            return SplitHorizontalLength(SplitToNewLines());
        }
        /*
        private static List<(string Word, IRendererHash Hash)[]> SplitToNewLines(LinkedList<HashedTextPiece> hashedNodes)
        {
            List<(string Word, IRendererHash Hash)[]> hashedWordLines = new List<(string Word, IRendererHash Hash)[]>();
            List<(string Word, IRendererHash Hash)> currentLine = new List<(string, IRendererHash)>();

            for (var node = hashedNodes.First; node != null; node = node.Next)
            {
                int[] newLineIndex = ProcesserHelpers.AllNewLineIndexes(node.Value.Text);
                if (newLineIndex.Length == 0)
                {
                    AddWordsToCurrentLine(node.Value.Text, ' ', node.Value.RendererHash);
                    continue;
                }

                int lastStart = 0;
                foreach (var index in newLineIndex)
                {
                    AddWordsToCurrentLine(node.Value.Text.Substring(lastStart, index - lastStart), ' ', node.Value.RendererHash);
                    hashedWordLines.Add(currentLine.ToArray());

                    currentLine.Clear();
                    lastStart = index + 1;
                }

                AddWordsToCurrentLine(node.Value.Text.Substring(lastStart), '\n', node.Value.RendererHash);
            }

            if (currentLine.Count != 0)
            {
                hashedWordLines.Add(currentLine.ToArray());
            }

            return hashedWordLines;

            void AddWordsToCurrentLine(string str, char seperator, IRendererHash hash)
            {
                string[] words = str.Split(seperator);
                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i] == "" && (i == 0 || i == words.Length - 1))
                    {
                        continue;
                    }

                    currentLine.Add((words[i], hash));
                }
            }
        }*/

        private List<(ITextPiece[] Parts, float LineWidth)> SplitHorizontalLength(List<ITextPiece[]> lineParts)
        {
            List<ITextPiece> currentLine = new List<ITextPiece>();
            List<(ITextPiece[] Parts, float LineWidth)> lines = new List<(ITextPiece[] Parts, float LineWidth)>();

            float spaceWidth = WordSpacing;
            float lineWidth = FirstRowOffset;
            foreach (var line in lineParts)
            {
                int lastStart = 0;

                for (int i = 0; i < line.Length; i++)
                {
                    float wordWidth = line[i].TextWidth();

                    if (lineWidth + wordWidth + (i - lastStart) * spaceWidth <= HorizontalTextLength)
                    {
                        currentLine.Add(line[i]);
                        lineWidth += wordWidth;
                    }
                    else
                    {
                        lines.Add((currentLine.ToArray(), lineWidth + (i - lastStart) * spaceWidth));
                        currentLine.Clear();

                        lastStart = i;
                        lineWidth = wordWidth;
                        currentLine.Add(line[i]);
                    }
                }

                lines.Add((currentLine.ToArray(), lineWidth + (line.Length - 1 - lastStart) * spaceWidth));

                currentLine.Clear();
                lineWidth = 0;
            }

            return lines;
        }

        /*private List<(HashedTextPiece[] Parts, float LineWidth)> SplitHorizontalLength(List<(string Word, IRendererHash Hash)[]> lineParts)
        {
            List<HashedTextPiece> currentLine = new List<HashedTextPiece>();
            List<(HashedTextPiece[] Parts, float LineWidth)> lines = new List<(HashedTextPiece[] Parts, float LineWidth)>();

            float spaceWidth = WordSpacing;
            float lineWidth = FirstRowOffset;
            foreach (var line in lineParts)
            {
                int lastStart = 0;

                for (int i = 0; i < line.Length; i++)
                {
                    (string word, IRendererHash hash) = line[i];
                    float wordWidth = hash.GetWidth(word);

                    if (lineWidth + wordWidth + (i - lastStart) * spaceWidth <= HorizontalTextLength)
                    {
                        currentLine.Add(new HashedTextPiece(word, hash));
                        lineWidth += wordWidth;
                    }
                    else
                    {
                        lines.Add((currentLine.ToArray(), lineWidth + (i - lastStart) * spaceWidth));
                        currentLine.Clear();

                        lastStart = i;
                        lineWidth = wordWidth;
                        currentLine.Add(new HashedTextPiece(word, hash));
                    }
                }

                lines.Add((currentLine.ToArray(), lineWidth + (line.Length - 1 - lastStart) * spaceWidth));

                currentLine.Clear();
                lineWidth = 0;
            }

            return lines;
        }

        /*public Vector2 GetTextSize()
        {
            List<(string Word, IRendererHash Hash)[]> existingLines = SplitToNewLines(SplitToHashes());

            int lineCount = 1;
            float maxWidth = 0;

            float spaceWidth = WordSpacing;
            foreach (var line in existingLines)
            {
                float lineWidth = 0;
                int lastStart = 0;

                for (int i = 0; i < line.Length; i++)
                {
                    (string word, IRendererHash hash) = line[i];
                    float wordWidth = hash.GetWidth(word);

                    if (lineWidth + wordWidth + (i - lastStart) * spaceWidth <= HorizontalTextLength)
                    {
                        lineWidth += wordWidth;
                    }
                    else
                    {
                        maxWidth = Math.Max(maxWidth, lineWidth + (i - lastStart) * spaceWidth);

                        lineCount++;
                        lastStart = i;
                        lineWidth = wordWidth;
                    }
                }

                maxWidth = Math.Max(maxWidth, lineWidth + (line.Length - 1 - lastStart) * spaceWidth);
            }

            if (HorizontalTextLength != int.MaxValue)
            {
                return new Vector2(HorizontalTextLength, LineSpacing * lineCount);
            }

            return new Vector2(maxWidth, LineSpacing * lineCount);
        }*/
    }
}
