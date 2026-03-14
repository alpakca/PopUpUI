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
    public class FixedSizeTextRenderer : DefaultRenderer
    {
        private readonly BatchDrawOptions drawOptions;

        public FixedSizeTextRenderer(int horizontalTextLength, SpriteFont font) : this(horizontalTextLength, font, null)
        {
        }

        public FixedSizeTextRenderer(int horizontalTextLength, SpriteFont font, BatchDrawOptions? options) : base(font)
        {
            ArgumentNullException.ThrowIfNull(font, nameof(font));

            HorizontalTextLength = horizontalTextLength;
            drawOptions = options ?? BatchDrawOptions.Default;
        }

        public override void Render(RenderTarget2D target, Vector2 offset)
        {
            DrawHelper.SetBeginBatch(target, drawOptions);

            List<(string[] LineWords, float LineWidth)> alignedWords = GetAlignedWords();
            offset += TextOffset.ToVector2() + GetAlignmentOffset(new Point(target.Width, target.Height), TextSizeFromAligned(alignedWords));

            foreach ((string[] lineWords, float lineWidth) in alignedWords)
            {
                float xOffset = 0;
                float wordSpacing = GetWordSpacing(lineWidth, lineWords.Length);

                foreach (var word in lineWords)
                {
                    DrawHelper.Batch.DrawString(Font, word, offset + new Vector2(xOffset, 0), TextColor * Opacity);
                    xOffset += (int)GetWidth(word) + wordSpacing;
                }

                offset.Y += LineSpacing;
            }

            DrawHelper.EndBatch();
        }

        public override Vector2 GetTextSize()
        {
            float minimumSpaceWidth = WordSpacing;
            int lineCount = 1;

            foreach (string existingLine in Text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                float lineWidth = 0;
                Span<string> words = existingLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                int lastStart = 0;
                for (int i = 0; i < words.Length; i++)
                {
                    float wordWidth = GetWidth(words[i]);

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

        public override int GetVisibleCharCount()
        {
            return GetAlignedWords().Sum(tuple => tuple.LineWords.Sum((Func<string, int>)(word => word.Length + 1))) - 1;
        }

        private Point TextSizeFromAligned(List<(string[] LineWords, float LineWidth)> lines)
        {
            return new Point(HorizontalTextLength, LineSpacing * lines.Count);
        }

        private List<(string[] LineWords, float LineWidth)> GetAlignedWords()
        {
            List<(string[] LineWords, float LineWidth)> lines = new List<(string[] LineWords, float LineWidth)>();

            float minimumSpaceWidth = WordSpacing;

            foreach (string existingLine in Text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                float lineWidth = 0;
                Span<string> words = existingLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                int lastStart = 0;
                for (int i = 0; i < words.Length; i++)
                {
                    float wordWidth = GetWidth(words[i]);

                    if (lineWidth + wordWidth + (i - lastStart) * minimumSpaceWidth <= HorizontalTextLength)
                    {
                        lineWidth += wordWidth;
                    }
                    else
                    {
                        lines.Add((words[lastStart..i].ToArray(), lineWidth));
                        lastStart = i;

                        lineWidth = wordWidth;
                    }
                }

                lines.Add((words[lastStart..].ToArray(), lineWidth));
            }

            return lines;
        }

        public override Vector2 ScreenCaretPosition(int caretPosition, Point? targetSize)
        {
            List<(string[] LineWords, float LineWidth)> lines = GetAlignedWords();

            int charConsumed = 0;

            int lineIndex = 0;
            int wordIndex = -1;
            foreach (var (lineWords, _) in lines)
            {
                for (int i = 0; i < lineWords.Length; i++)
                {
                    if (caretPosition <= charConsumed + lineWords[i].Length)
                    {
                        wordIndex = i;
                        break;
                    }

                    charConsumed += lineWords[i].Length + 1; // +1 for new line char
                }

                if (wordIndex != -1)
                {
                    break;
                }

                lineIndex++;
            }

            float wordSpacing = GetWordSpacing(lines[lineIndex].LineWidth, lines[lineIndex].LineWords.Length);

            // length of words and spaces until the word that caret in
            float xOffset = lines[lineIndex].LineWords.Take(wordIndex).Sum(word => Font.MeasureString(word).X + wordSpacing);

            // length of word until caret position
            xOffset += GetWidth(lines[lineIndex].LineWords[wordIndex].Substring(0, caretPosition - charConsumed));

            if (targetSize.HasValue)
            {
                return new Vector2(xOffset, LineSpacing * lineIndex)
                     + TextOffset.ToVector2()
                     + GetAlignmentOffset(targetSize.Value, TextSizeFromAligned(lines));
            }

            return new Vector2(xOffset, LineSpacing * lineIndex)
                 + TextOffset.ToVector2();
        }

        public override int MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize)
        {
            List<(string[] LineWords, float LineWidth)> lines = GetAlignedWords();

            if (targetSize.HasValue)
            {
                mouseOnTarget -= GetAlignmentOffset(targetSize.Value, TextSizeFromAligned(lines));
            }

            mouseOnTarget -= TextOffset.ToVector2();

            int lineIndex = (int)(mouseOnTarget.Y / LineSpacing);
            float wordSpacing = GetWordSpacing(lines[lineIndex].LineWidth, lines[lineIndex].LineWords.Length);

            int charsUntilLine = lines.Take(lineIndex).Sum(tuple => AllWordLengths(tuple.LineWords) + 1); // +1 for newline chars
            int caretLineWordLength = AllWordLengths(lines[lineIndex].LineWords);

            // shortcut for selecting the last character of line if line width is smaller than mouse position
            // LineWidth does NOT contain space character length, add it too for comparison
            if (lines[lineIndex].LineWidth + wordSpacing * (lines[lineIndex].LineWords.Length - 1) < mouseOnTarget.X)
            {
                return charsUntilLine + caretLineWordLength;
            }

            int consumedChars = 0;
            float currentWidth = 0;

            foreach (var word in lines[lineIndex].LineWords)
            {
                for (int i = 0; i < word.Length; i++)
                {
                    if (currentWidth + GetWidth(word.Substring(0, i)) + GetWidth(word[i].ToString()) / 2 > mouseOnTarget.X)
                    {
                        return charsUntilLine + consumedChars + i;
                    }
                }

                currentWidth += GetWidth(word);

                if (currentWidth + wordSpacing / 2 > mouseOnTarget.X)
                {
                    return charsUntilLine + consumedChars + word.Length;
                }

                currentWidth += wordSpacing;
                consumedChars += word.Length + 1;
            }

            // should not reach this point - already handled in shortcut part
            return charsUntilLine + caretLineWordLength;
        }

        private static int AllWordLengths(string[] words)
        {
            return words.Sum(word => word.Length) 
                 + words.Length - 1;  // for space characters
        }

        private float GetWordSpacing(float lineWidth, int wordCount)
        {
            return wordCount == 0 ? 0 : (int)((HorizontalTextLength - lineWidth) / (wordCount - 1));
        }
    }
}
