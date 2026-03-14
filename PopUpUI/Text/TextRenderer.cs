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
    /// <summary>Presents an object that can render text on <see cref="RenderTarget2D"/> in different ways.</summary>
    public class TextRenderer : DefaultRenderer
    {
        private readonly BatchDrawOptions drawOptions;

        /// <summary>Gets or sets the orientation of the text. Default value is <see cref="TextOrientation.Left"/>.</summary>
        public TextOrientation Orientation { get; set; } = TextOrientation.Left;

        /// <summary>Initializes a new instance of <see cref="TextRenderer"/> class with given <paramref name="font"/> and default <see cref="BatchDrawOptions"/>.</summary>
        /// <param name="font"><see cref="SpriteFont"/> that will be used while rendering text.</param>
        public TextRenderer(SpriteFont font) : this(font, null)
        {
        }

        /// <summary>Initializes a new instance of <see cref="TextRenderer"/> class 
        /// with given <paramref name="font"/> and <paramref name="options"/> to use while drawing.</summary>
        /// <param name="font"><see cref="SpriteFont"/> that will be used while rendering text.</param>
        /// <param name="options"><see cref="BatchDrawOptions"/> that will be used while rendering text.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TextRenderer(SpriteFont font, BatchDrawOptions? options) : base(font)
        {
            ArgumentNullException.ThrowIfNull(font, nameof(font));

            drawOptions = options ?? BatchDrawOptions.Default;
        }

        /// <summary>Initializes a new instance of <see cref="TextRenderer"/> class with properties from given <paramref name="renderer"/>.</summary>
        /// <param name="renderer">A <see cref="TextRenderer"/> instance to copy properties from.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TextRenderer(TextRenderer renderer) : base(renderer.Font)
        {
            ArgumentNullException.ThrowIfNull(renderer, nameof(renderer));

            drawOptions = renderer.drawOptions;

            Text = renderer.Text;
            HorizontalTextLength = renderer.HorizontalTextLength;
            Opacity = renderer.Opacity;
            Orientation = renderer.Orientation;
            Alignment = renderer.Alignment;
            TextOffset = renderer.TextOffset;
            TextColor = renderer.TextColor;
        }

        public override void Render(RenderTarget2D target, Vector2 offset)
        {
            // right or middle orientations will cause a too large x offset that text won't appear, with maximum horizontal text length
            if (Orientation != TextOrientation.Left && HorizontalTextLength == int.MaxValue)
            {
                throw new ArgumentException("Maximum HorizontalTextLength should only be used with TextOrientation.Left");
            }

            DrawHelper.SetBeginBatch(target, drawOptions);

            List<(string Line, float LineWidth)> modifiedText = GetModifiedText();
            offset += TextOffset.ToVector2() + GetAlignmentOffset(new Point(target.Width, target.Height), TextSizeFromModified(modifiedText));

            foreach ((string line, float lineWidth) in modifiedText)
            {
                DrawHelper.Batch.DrawString(Font, line, offset + GetOrientationOffset(lineWidth), TextColor * Opacity);
                offset.Y += LineSpacing;
            }

            DrawHelper.EndBatch();
        }

        public override int GetVisibleCharCount()
        {
            return Text.Length;
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

        private Point TextSizeFromModified(List<(string Line, float LineWidth)> lines)
        {
            if (HorizontalTextLength != int.MaxValue)
            {
                return new Point(HorizontalTextLength, LineSpacing * lines.Count);
            }

            return new Point((int)lines.Max(tuple => tuple.LineWidth), LineSpacing * lines.Count);
        }

        public override Vector2 GetTextSize()
        {
            float spaceWidth = WordSpacing;

            int lineCount = 1;
            float maxWidth = 0;

            foreach (var existingLine in Text.Split('\n'))
            {
                float lineWidth = 0;
                string[] words = existingLine.Split(' ');

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
                        maxWidth = Math.Max(maxWidth, lineWidth + (i - lastStart) * spaceWidth);

                        lineCount++;
                        lastStart = i;
                        lineWidth = wordWidth;
                    }
                }

                maxWidth = Math.Max(maxWidth, lineWidth + (words.Length - 1 - lastStart) * spaceWidth);
            }

            if (HorizontalTextLength == int.MaxValue)
            {
                return new Vector2(HorizontalTextLength, LineSpacing * lineCount);
            }

            return new Vector2(maxWidth, LineSpacing * lineCount);
        }

        private List<(string Line, float LineWidth)> GetModifiedText()
        {
            float spaceWidth = WordSpacing;
            List<(string Line, float LineWidth)> lines = new List<(string Line, float LineWidth)>();

            foreach (var existingLine in Text.Split('\n'))
            {
                float lineWidth = 0;
                Span<string> words = existingLine.Split(' ');

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
                        lines.Add((string.Join(' ', words.Slice(lastStart, i - lastStart)!), lineWidth + (i - lastStart - 1) * spaceWidth));

                        lastStart = i;
                        lineWidth = wordWidth;
                    }
                }

                lines.Add((string.Join(' ', words.Slice(lastStart)!), lineWidth + (words.Length - 1 - lastStart - 1) * spaceWidth));
            }

            return lines;
        }

        public override Vector2 ScreenCaretPosition(int caretPosition, Point? targetSize)
        {
            List<(string Line, float LineWidth)> lines = GetModifiedText();

            int charConsumed = 0;
            int lineIndex = 0;
            for (; lineIndex < lines.Count; lineIndex++)
            {
                if (caretPosition <= charConsumed + lines[lineIndex].Line.Length)
                {
                    break;
                }

                charConsumed += lines[lineIndex].Line.Length + 1; // +1 for newline char
            }

            if (targetSize.HasValue)
            {
                return new Vector2(Font.MeasureString(lines[lineIndex].Line.Substring(0, caretPosition - charConsumed)).X, LineSpacing * lineIndex)
                     + TextOffset.ToVector2()
                     + GetOrientationOffset(lines[lineIndex].LineWidth)
                     + GetAlignmentOffset(targetSize.Value, TextSizeFromModified(lines));
            }

            return new Vector2(Font.MeasureString(lines[lineIndex].Line.Substring(0, caretPosition - charConsumed)).X, LineSpacing * lineIndex)
                 + TextOffset.ToVector2()
                 + GetOrientationOffset(lines[lineIndex].LineWidth);
        }

        public override int MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize)
        {
            List<(string Line, float LineWidth)> lines = GetModifiedText();

            if (targetSize.HasValue)
            {
                mouseOnTarget -= GetAlignmentOffset(targetSize.Value, TextSizeFromModified(lines));
            }

            mouseOnTarget -= TextOffset.ToVector2();

            int lineIndex = (int)(mouseOnTarget.Y / LineSpacing);
            int charsUntilLine = lines.Take(lineIndex).Sum(tuple => tuple.Line.Length + 1); // +1 for newline chars

            mouseOnTarget -= GetOrientationOffset(lines[lineIndex].LineWidth);

            // shortcut for selecting the last character of line if line width is smaller than mouse position
            if (lines[lineIndex].LineWidth < mouseOnTarget.X)
            {
                return charsUntilLine + lines[lineIndex].Line.Length;
            }

            for (int i = 0; i < lines[lineIndex].Line.Length; i++)
            {                                                        
                                                                   // adjustment for half of the char length
                if (GetWidth(lines[lineIndex].Line.Substring(0, i)) + GetWidth(lines[lineIndex].Line[i].ToString()) / 2 > mouseOnTarget.X)
                {
                    return charsUntilLine + i;
                }
            }

            // should not reach this point - already handled in shortcut part
            return charsUntilLine + lines[lineIndex].Line.Length;
        }
    }
}
