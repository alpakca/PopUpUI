using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 8632

namespace PopUpUI.Text
{
    public class RichFixedSizeTextRenderer : RichRenderer
    {
        private readonly BatchDrawOptions drawOptions;

        public RichFixedSizeTextRenderer(int horizontalTextLength, SpriteFont defaultFont) : this(horizontalTextLength, defaultFont, null)
        {
        }

        public RichFixedSizeTextRenderer(int horizontalTextLength, SpriteFont defaultFont, BatchDrawOptions? options)
            : base(Color.White, defaultFont)
        {
            ArgumentNullException.ThrowIfNull(defaultFont);

            HorizontalTextLength = horizontalTextLength;
            drawOptions = options ?? BatchDrawOptions.Default;
        }

        public RichFixedSizeTextRenderer(RichFixedSizeTextRenderer renderer) : base(renderer.defaultTextHash.Color, renderer.defaultTextHash.Font)
        {
            ArgumentNullException.ThrowIfNull(renderer);

            drawOptions = renderer.drawOptions;

            Text = renderer.Text;
            HorizontalTextLength = renderer.HorizontalTextLength;
            Opacity = renderer.Opacity;
            Alignment = renderer.Alignment;
            TextOffset = renderer.TextOffset;

            _hashes.AddRange(renderer._hashes);
        }

        public override void Render(RenderTarget2D target, Vector2 offset)
        {
            DrawHelper.SetBeginBatch(target, drawOptions);

            List<(HashedTextPiece[] Parts, float TotalWordsWidthWidth)> hashedLines = GetHashedLines();
            offset += TextOffset.ToVector2() + GetAlignmentOffset(new Point(target.Width, target.Height), TextSizeFromHashedLines(hashedLines));

            foreach ((HashedTextPiece[] line, float lineWidth) in hashedLines)
            {
                float xOffset = 0;

                float wordSpacing = GetWordSpacing(lineWidth, line.Length);
                foreach (HashedTextPiece hashedPiece in line)
                {
                    hashedPiece.DrawHash(offset + new Vector2(xOffset, 0));
                    xOffset += hashedPiece.TextWidth() + wordSpacing;
                }

                offset.Y += LineSpacing;
            }

            DrawHelper.EndBatch();
        }

        public override int GetVisibleCharCount()
        {
            return GetHashedLines().Sum(tuple => tuple.Parts.Sum((Func<HashedTextPiece, int>)(piece => piece.VisibleCharLength + 1))) - 1;
        }

        private List<(HashedTextPiece[] Parts, float TotalWordsWidth)> GetHashedLines()
        {
            return SplitHorizontalLength(SplitToNewLines(SplitToHashes()));
        }

        protected static List<(string Word, IRendererHash Hash)[]> SplitToNewLines(LinkedList<HashedTextPiece> hashedNodes)
        {
            List<(string Word, IRendererHash Hash)[]> hashedWordLines = new List<(string Word, IRendererHash Hash)[]>();
            List<(string Word, IRendererHash Hash)> currentLine = new List<(string, IRendererHash)>();

            for (var node = hashedNodes.First; node != null; node = node.Next)
            {
                int[] newLineIndex = AllNewLineIndexes(node.Value.Text);
                if (newLineIndex.Length == 0)
                {
                    foreach (var word in node.Value.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    {
                        currentLine.Add((word, node.Value.RendererHash));
                    }

                    continue;
                }

                int lastStart = 0;
                foreach (var index in newLineIndex)
                {
                    foreach (var word in node.Value.Text.Substring(lastStart, index - lastStart).Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    {
                        currentLine.Add((word, node.Value.RendererHash));
                    }

                    hashedWordLines.Add(currentLine.ToArray());

                    currentLine.Clear();
                    lastStart = index + 1;
                }

                foreach (var word in node.Value.Text.Substring(lastStart).Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    currentLine.Add((word, node.Value.RendererHash));
                }
            }

            if (currentLine.Count != 0)
            {
                hashedWordLines.Add(currentLine.ToArray());
            }

            return hashedWordLines;
        }

        private List<(HashedTextPiece[] Parts, float TotalWordsWidth)> SplitHorizontalLength(List<(string Word, IRendererHash Hash)[]> lineParts)
        {
            List<HashedTextPiece> currentLine = new List<HashedTextPiece>();
            List<(HashedTextPiece[] Parts, float LineWidth)> lines = new List<(HashedTextPiece[] Parts, float LineWidth)>();

            float minimumSpaceWidth = WordSpacing;
            foreach (var line in lineParts)
            {
                float lineWidth = 0;
                int lastStart = 0;

                for (int i = 0; i < line.Length; i++)
                {
                    (string word, IRendererHash hash) = line[i];
                    float wordWidth = hash.GetWidth(word);

                    if (lineWidth + wordWidth + (i - lastStart) * minimumSpaceWidth <= HorizontalTextLength)
                    {
                        currentLine.Add(new HashedTextPiece(word, hash));
                        lineWidth += wordWidth;
                    }
                    else
                    {
                        lines.Add((currentLine.ToArray(), lineWidth));
                        currentLine.Clear();

                        lastStart = i;
                        lineWidth = wordWidth;
                        currentLine.Add(new HashedTextPiece(word, hash));
                    }
                }

                lines.Add((currentLine.ToArray(), lineWidth));
                currentLine.Clear();
            }

            return lines;
        }

        public override Vector2 GetTextSize()
        {
            List<(string Word, IRendererHash Hash)[]> existingLines = SplitToNewLines(SplitToHashes());

            float minimumSpaceWidth = WordSpacing;
            int lineCount = 1;

            foreach (var line in existingLines)
            {
                float lineWidth = 0;
                int lastStart = 0;

                for (int i = 0; i < line.Length; i++)
                {
                    (string word, IRendererHash hash) = line[i];

                    float wordWidth = hash.GetWidth(word);

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

            float wordSpacing = GetWordSpacing(lines[lineIndex].LineWidth, lines[lineIndex].Parts.Length);

            // length of words and spaces until the word that caret in
            float xOffset = lines[lineIndex].Parts.Take(wordIndex).Sum(part => part.TextWidth() + wordSpacing);

            // length of word until caret position
            xOffset += lines[lineIndex].Parts[wordIndex].TextWidth(0, caretPosition - charConsumed);

            if (targetSize.HasValue)
            {
                return new Vector2(xOffset, LineSpacing * lineIndex)
                     + TextOffset.ToVector2()
                     + GetAlignmentOffset(targetSize.Value, TextSizeFromHashedLines(lines));
            }

            return new Vector2(xOffset, LineSpacing * lineIndex)
                 + TextOffset.ToVector2();
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
            float wordSpacing = GetWordSpacing(lines[lineIndex].LineWidth, lines[lineIndex].Parts.Length);

            int charsUntilLine = lines.Take(lineIndex).Sum(tuple => AllWordLengths(tuple.Parts) + 1); // +1 for newline chars
            int caretLineWordLength = AllWordLengths(lines[lineIndex].Parts);

            // shortcut for selecting the last character of line if line width is smaller than mouse position
            if (lines[lineIndex].LineWidth + wordSpacing * (lines[lineIndex].Parts.Length - 1) < mouseOnTarget.X)
            {
                return charsUntilLine + caretLineWordLength;
            }

            int consumedChars = 0;
            float currentWidth = 0;

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

                if (currentWidth + wordSpacing / 2 > mouseOnTarget.X)
                {
                    return charsUntilLine + consumedChars + part.VisibleCharLength;
                }

                currentWidth += wordSpacing;
                consumedChars += part.VisibleCharLength + 1;
            }

            // should not reach this point - already handled in shortcut
            return charsUntilLine + caretLineWordLength;
        }

        private float GetWordSpacing(float lineWidth, int wordCount)
        {
            return wordCount == 0 ? 0 : (HorizontalTextLength - lineWidth) / (wordCount - 1);
        }

        private static int AllWordLengths(HashedTextPiece[] words)
        {
            return words.Sum(word => word.VisibleCharLength)
                 + words.Length - 1; // for space characters between words
        }
    }
}
