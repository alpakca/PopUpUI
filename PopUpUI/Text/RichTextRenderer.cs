using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 8524
#pragma warning disable 8632

namespace PopUpUI.Text
{
    public class RichTextRenderer : RichRenderer
    {
        private readonly BatchDrawOptions drawOptions;

        /// <summary>Gets or sets the orientation of the text. Default value is <see cref="TextOrientation.Left"/>.</summary>
        public TextOrientation Orientation { get; set; } = TextOrientation.Left;

        public RichTextRenderer(SpriteFont defaultFont) : this(defaultFont, null)
        {
        }

        public RichTextRenderer(SpriteFont defaultFont, BatchDrawOptions? options) : base(Color.White, defaultFont)
        {
            ArgumentNullException.ThrowIfNull(defaultFont);

            drawOptions = options ?? BatchDrawOptions.Default;
        }

        public RichTextRenderer(RichTextRenderer renderer) : base(renderer.defaultTextHash.Color, renderer.defaultTextHash.Font)
        {
            ArgumentNullException.ThrowIfNull(renderer);

            drawOptions = renderer.drawOptions;

            Text = renderer.Text;
            HorizontalTextLength = renderer.HorizontalTextLength;
            Opacity = renderer.Opacity;
            Orientation = renderer.Orientation;
            Alignment = renderer.Alignment;
            TextOffset = renderer.TextOffset;

            _hashes.AddRange(renderer._hashes);
        }

        public override void Render(RenderTarget2D target, Vector2 offset)
        {
            // right or middle orientations will cause a too large x offset that text won't appear, with maximum horizontal text length
            if (Orientation != TextOrientation.Left && HorizontalTextLength == int.MaxValue)
            {
                throw new ArgumentException("Maximum HorizontalTextLength should only be used with TextOrientation.Left");
            }

            DrawHelper.SetBeginBatch(target, drawOptions);

            List<(HashedTextPiece[] Parts, float LineWidth)> hashedLines = GetHashedLines();
            offset += TextOffset.ToVector2() + GetAlignmentOffset(new Point(target.Width, target.Height), TextSizeFromHashedLines(hashedLines));

            float spaceWidth = WordSpacing;
            foreach ((HashedTextPiece[] line, float lineWidth) in hashedLines)
            {
                Vector2 lineOffset = GetOrientationOffset(lineWidth);

                foreach (HashedTextPiece hashedPiece in line)
                {
                    hashedPiece.DrawHash(offset + lineOffset);
                    lineOffset.X += hashedPiece.TextWidth() + spaceWidth;
                }

                offset.Y += LineSpacing;
            }

            DrawHelper.EndBatch();
        }

        public override int GetVisibleCharCount()
        {
            return SplitToHashes().Sum(part => part.VisibleCharLength);
        }

        private Vector2 GetOrientationOffset(float lineWidth)
        {
            return Orientation switch
            {
                TextOrientation.Left => new Vector2(),
                TextOrientation.Middle => new Vector2((HorizontalTextLength - (int)lineWidth) / 2, 0),
                TextOrientation.Right => new Vector2(HorizontalTextLength - (int)lineWidth, 0),
            };
        }

        private List<(HashedTextPiece[] Parts, float LineWidth)> GetHashedLines()
        {
            return SplitHorizontalLength(SplitToNewLines(SplitToHashes()));
        }

        private static List<(string Word, IRendererHash Hash)[]> SplitToNewLines(LinkedList<HashedTextPiece> hashedNodes)
        {
            List<(string Word, IRendererHash Hash)[]> hashedWordLines = new List<(string Word, IRendererHash Hash)[]>();
            List<(string Word, IRendererHash Hash)> currentLine = new List<(string, IRendererHash)>();

            for (var node = hashedNodes.First; node != null; node = node.Next)
            {
                int[] newLineIndex = AllNewLineIndexes(node.Value.Text);
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
        }

        private List<(HashedTextPiece[] Parts, float LineWidth)> SplitHorizontalLength(List<(string Word, IRendererHash Hash)[]> lineParts)
        {
            List<HashedTextPiece> currentLine = new List<HashedTextPiece>();
            List<(HashedTextPiece[] Parts, float LineWidth)> lines = new List<(HashedTextPiece[] Parts, float LineWidth)>();

            float spaceWidth = WordSpacing;
            foreach (var line in lineParts)
            {
                float lineWidth = 0;
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
            }

            return lines;
        }

        public override Vector2 GetTextSize()
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
        }

        public override Vector2 ScreenCaretPosition(int caretPosition, Point? targetSize)
        {
            List<(HashedTextPiece[] Parts, float LineWidth)> lines = GetHashedLines();

            int charConsumed = 0;

            int lineIndex = 0;
            int wordIndex = -1;
            foreach (var (lineWords, _) in lines)
            {
                for (int i = 0; i < lineWords.Length; i++)
                {
                    if (caretPosition <= charConsumed + lineWords[i].VisibleCharLength)
                    {
                        wordIndex = i;
                        break;
                    }

                    // +1 for space char, or new line char if it is the last word of line
                    charConsumed += lineWords[i].VisibleCharLength + 1;
                }

                if (wordIndex != -1)
                {
                    break;
                }

                lineIndex++;
            }

            float spaceWidth = WordSpacing;

            // length of words and spaces until the word that caret in
            float xOffset = lines[lineIndex].Parts.Take(wordIndex).Sum(part => part.TextWidth() + spaceWidth);

            // length of word until caret position
            xOffset += lines[lineIndex].Parts[wordIndex].TextWidth(0, caretPosition - charConsumed);

            if (targetSize.HasValue)
            {
                return new Vector2(xOffset, LineSpacing * lineIndex)
                     + TextOffset.ToVector2()
                     + GetOrientationOffset(lines[lineIndex].LineWidth)
                     + GetAlignmentOffset(targetSize.Value, TextSizeFromHashedLines(lines));
            }

            return new Vector2(xOffset, LineSpacing * lineIndex)
                 + TextOffset.ToVector2()
                 + GetOrientationOffset(lines[lineIndex].LineWidth);
        }

        public override int MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize)
        {
            List<(HashedTextPiece[] Parts, float LineWidth)> lines = GetHashedLines();

            if (targetSize.HasValue)
            {
                mouseOnTarget -= GetAlignmentOffset(targetSize.Value, TextSizeFromHashedLines(lines));
            }

            mouseOnTarget -= TextOffset.ToVector2();

            int lineIndex = (int)(mouseOnTarget.Y / LineSpacing);

            int charsUntilLine = lines.Take(lineIndex).Sum(tuple => AllWordLengths(tuple.Parts) + 1); // +1 for newline chars
            int caretLineWordLength = AllWordLengths(lines[lineIndex].Parts);

            mouseOnTarget -= GetOrientationOffset(lines[lineIndex].LineWidth);

            // shortcut for selecting the last character of line if line width is smaller than mouse position
            if (lines[lineIndex].LineWidth < mouseOnTarget.X)
            {
                return charsUntilLine + caretLineWordLength;
            }

            int consumedChars = 0;
            float currentWidth = 0;

            float spaceWidth = WordSpacing;
            foreach (var part in lines[lineIndex].Parts)
            {
                for (int i = 0; i < part.VisibleCharLength; i++)
                {
                    if (currentWidth + part.TextWidth(0, i) + part.TextWidth(i, 1) / 2 > mouseOnTarget.X)
                    {
                        return charsUntilLine + consumedChars + i;
                    }
                }

                currentWidth += part.TextWidth();

                if (currentWidth + spaceWidth / 2 > mouseOnTarget.X)
                {
                    return charsUntilLine + consumedChars + part.VisibleCharLength;
                }

                currentWidth += spaceWidth;
                consumedChars += part.VisibleCharLength + 1;
            }

            // should not reach this point - already handled in shortcut
            return charsUntilLine + caretLineWordLength;
        }

        private static int AllWordLengths(HashedTextPiece[] words)
        {
            return words.Sum(word => word.VisibleCharLength) 
                 + words.Length - 1; // for space characters
        }
    }
}
