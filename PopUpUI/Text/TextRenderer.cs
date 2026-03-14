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
    /// <summary>Presents an object that can render text on <see cref="RenderTarget2D"/> in different ways.</summary>
    public class TextRenderer : ITextRenderer
    {
        private readonly BatchDrawOptions drawOptions;

        /// <summary>Gets or sets maximum length of each row of text can contain. Default value is <see cref="int.MaxValue"/>.</summary>
        public int HorizontalTextLength { get; set; } = int.MaxValue;
        /// <summary>Gets or sets the opacity of the text. Expected value is between 0.0f and 1.0f. Default value is 1.0f.</summary>
        public float Opacity { get; set; } = 1;
        /// <summary>Gets or sets the text that will be rendered.</summary>
        public string Text { get; set; } = "";

        /// <summary>Gets or sets the orientation of the text. Default value is <see cref="TextOrientation.Left"/>.</summary>
        public TextOrientation Orientation { get; set; } = TextOrientation.Left;
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
        public TextRenderer(SpriteFont font, BatchDrawOptions? options)
        {
            ArgumentNullException.ThrowIfNull(font, nameof(font));

            Font = font;
            drawOptions = options ?? BatchDrawOptions.Default;
        }

        /// <summary>Initializes a new instance of <see cref="TextRenderer"/> class with properties from given <paramref name="renderer"/>.</summary>
        /// <param name="renderer">A <see cref="TextRenderer"/> instance to copy properties from.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TextRenderer(TextRenderer renderer)
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

            Font = renderer.Font;
        }

        public void Render(RenderTarget2D target, Vector2 offset)
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
                offset.Y += Font.LineSpacing;
            }

            DrawHelper.EndBatch();
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

        private Point TextSizeFromModified(List<(string Line, float LineWidth)> lines)
        {
            if (HorizontalTextLength != int.MaxValue)
            {
                return new Point(HorizontalTextLength, Font.LineSpacing * lines.Count);
            }

            return new Point((int)lines.Max(tuple => tuple.LineWidth), Font.LineSpacing * lines.Count);
        }

        public Vector2 GetTextSize()
        {
            float spaceWidth = Font.MeasureString(" ").X;

            int lineCount = 1;
            float maxWidth = 0;

            foreach (var existingLine in Text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                float lineWidth = 0;
                string[] words = existingLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

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
                return new Vector2(HorizontalTextLength, Font.LineSpacing * lineCount);
            }

            return new Vector2(maxWidth, Font.LineSpacing * lineCount);
        }

        private List<(string Line, float LineWidth)> GetModifiedText()
        {
            float spaceWidth = Font.MeasureString(" ").X;
            List<(string Line, float LineWidth)> lines = new List<(string Line, float LineWidth)>();

            foreach (var existingLine in Text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
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
                        lines.Add((string.Join(' ', words.Slice(lastStart, i - lastStart)!), lineWidth + (i - lastStart - 1) * spaceWidth));

                        lastStart = i;
                        lineWidth = wordWidth;
                    }
                }

                lines.Add((string.Join(' ', words.Slice(lastStart)!), lineWidth + (words.Length - 1 - lastStart - 1) * spaceWidth));
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
                return new Vector2(Font.MeasureString(lines[lineIndex].Line.Substring(0, caretPosition - charConsumed)).X, Font.LineSpacing * lineIndex)
                     + TextOffset.ToVector2()
                     + GetOrientationOffset(lines[lineIndex].LineWidth)
                     + GetAlignmentOffset(targetSize.Value, TextSizeFromModified(lines));
            }

            return new Vector2(Font.MeasureString(lines[lineIndex].Line.Substring(0, caretPosition - charConsumed)).X, Font.LineSpacing * lineIndex)
                 + TextOffset.ToVector2()
                 + GetOrientationOffset(lines[lineIndex].LineWidth);
        }

        public int MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize)
        {
            List<(string Line, float LineWidth)> lines = GetModifiedText();

            if (targetSize.HasValue)
            {
                mouseOnTarget -= GetAlignmentOffset(targetSize.Value, TextSizeFromModified(lines));
            }

            mouseOnTarget -= TextOffset.ToVector2();

            int lineIndex = (int)(mouseOnTarget.Y / Font.LineSpacing);
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
