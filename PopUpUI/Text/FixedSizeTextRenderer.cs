using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
#pragma warning disable 8524

namespace PopUpUI.Text
{
    public class FixedSizeTextRenderer : ITextRenderer
    {
        private readonly BatchDrawOptions drawOptions;

        /// <summary>Gets or sets maximum length of each row of text can contain.</summary>
        public int HorizontalTextLength { get; set; }
        /// <summary>Gets or sets the opacity of the text. Expected value is between 0.0f and 1.0f. Default value is 1.0f.</summary>
        public float Opacity { get; set; } = 1;
        /// <summary>Gets or sets the text that will be rendered.</summary>
        public string Text { get; set; } = "";

        /// <summary>Gets or sets the alignment of the text, that is text's position relative to target it will be rendered to.
        /// Default value is <see cref="TextAlignment.TopLeft"/>.</summary>
        /// <remarks>This property does not have any effect if renderer creates a target that is not have a fixed size.</remarks>
        public TextAlignment Alignment { get; set; } = TextAlignment.TopLeft;
        /// <summary>Gets or sets an offset to be applied to the target text will be rendered to, with <see cref="Alignment"/>.</summary>
        /// <remarks>This property does not effect text size from <see cref="GetTextSize"/>. It can be used to intentionally creating wiped text.</remarks>
        public Point TextOffset { get; set; } = new Point();
        /// <summary>Gets or sets the color of the text. Default value is <see cref="Color.White"/>.</summary>
        public Color TextColor { get; set; } = Color.White;

        /// <summary>Gets the font that is used by renderer to create text.</summary>
        public SpriteFont Font { get; }

        public FixedSizeTextRenderer(int horizontalTextLength, SpriteFont font) : this(horizontalTextLength, font, null)
        {
        }

        public FixedSizeTextRenderer(int horizontalTextLength, SpriteFont font, BatchDrawOptions? options)
        {
            ArgumentNullException.ThrowIfNull(font, nameof(font));

            Font = font;
            HorizontalTextLength = horizontalTextLength;
            drawOptions = options ?? BatchDrawOptions.Default;
        }

        public void Render(RenderTarget2D target, Vector2 offset)
        {
            DrawHelper.SetBeginBatch(target, drawOptions);

            List<(string[] LineWords, float LineWidth)> alignedWords = GetAlignedWords();
            offset += TextOffset.ToVector2() + GetAlignmentOffset(new Point(target.Width, target.Height), TextSizeFromAligned(alignedWords));

            foreach ((string[] lineWords, float lineWidth) in alignedWords)
            {
                int xOffset = 0;
                int wordSpacing = (int)GetWordSpacing(lineWidth, lineWords.Length);

                foreach (var word in lineWords)
                {
                    DrawHelper.Batch.DrawString(Font, word, offset + new Vector2(xOffset, 0), TextColor * Opacity);
                    xOffset += (int)GetWidth(word) + wordSpacing;
                }

                offset.Y += Font.LineSpacing;
            }

            DrawHelper.EndBatch();
        }

        private Vector2 GetAlignmentOffset(Point surfaceSize, Point textSize)
        {
            int xOffset = Alignment switch
            {
                TextAlignment.TopLeft or TextAlignment.MiddleLeft or TextAlignment.BottomLeft => 0,
                TextAlignment.TopMiddle or TextAlignment.Center or TextAlignment.BottomMiddle => (surfaceSize.X - textSize.X) / 2,
                TextAlignment.TopRight or TextAlignment.MiddleRight or TextAlignment.BottomRight => surfaceSize.X - textSize.X
            };

            int yOffset = Alignment switch
            {
                TextAlignment.TopLeft or TextAlignment.TopMiddle or TextAlignment.TopRight => 0,
                TextAlignment.MiddleLeft or TextAlignment.Center or TextAlignment.MiddleRight => (surfaceSize.Y - textSize.Y) / 2,
                TextAlignment.BottomLeft or TextAlignment.BottomMiddle or TextAlignment.BottomRight => surfaceSize.Y - textSize.Y
            };

            return new Vector2(xOffset, yOffset);
        }

        public Vector2 GetTextSize()
        {
            float spaceWidth = Font.MeasureString(" ").X;
            int lineCount = 1;

            foreach (string existingLine in Text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                float lineWidth = 0;
                Span<string> words = existingLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                int lastStart = 0;
                for (int i = 0; i < words.Length; i++)
                {
                    float wordWidth = GetWidth(words[i]);

                    if (lineWidth + wordWidth + (i - lastStart) * spaceWidth <= HorizontalTextLength)
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

            return new Vector2(HorizontalTextLength, Font.LineSpacing * lineCount);
        }

        private Point TextSizeFromAligned(List<(string[] LineWords, float LineWidth)> lines)
        {
            return new Point(HorizontalTextLength, Font.LineSpacing * lines.Count);
        }

        private List<(string[] LineWords, float LineWidth)> GetAlignedWords()
        {
            List<(string[] LineWords, float LineWidth)> lines = new List<(string[] LineWords, float LineWidth)>();

            float spaceWidth = Font.MeasureString(" ").X;

            foreach (string existingLine in Text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                float lineWidth = 0;
                Span<string> words = existingLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                int lastStart = 0;
                for (int i = 0; i < words.Length; i++)
                {
                    float wordWidth = GetWidth(words[i]);

                    if (lineWidth + wordWidth + (i - lastStart) * spaceWidth <= HorizontalTextLength)
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

        private float GetWidth(string str)
        {
            return Font.MeasureString(str).X;
        }

        public int GetVisibleCharCount()
        {
            return Text.Length;
        }

        public Vector2 ScreenCaretPosition(int caretPosition, Point? targetSize)
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
                return new Vector2(xOffset, Font.LineSpacing * lineIndex)
                     + TextOffset.ToVector2()
                     + GetAlignmentOffset(targetSize.Value, TextSizeFromAligned(lines));
            }

            return new Vector2(xOffset, Font.LineSpacing * lineIndex)
                 + TextOffset.ToVector2();
        }

        public int MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize)
        {
            List<(string[] LineWords, float LineWidth)> lines = GetAlignedWords();

            if (targetSize.HasValue)
            {
                mouseOnTarget -= GetAlignmentOffset(targetSize.Value, TextSizeFromAligned(lines));
            }

            mouseOnTarget -= TextOffset.ToVector2();

            int lineIndex = (int)(mouseOnTarget.Y / Font.LineSpacing);
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
            return wordCount == 0 ? 0 : (HorizontalTextLength - lineWidth) / (wordCount - 1);
        }
    }
}
