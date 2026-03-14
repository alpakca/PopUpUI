using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

#pragma warning disable 8524
#pragma warning disable 8632

namespace PopUpUI.Text
{
    public class RichTextRenderer : ITextRenderer
    {
        private readonly BatchDrawOptions drawOptions;
        private readonly TextHash defaultTextHash;

        private string _regexPattern = "";
        private readonly List<IRendererHash> _hashes;

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
        public Color DefaultTextColor
        {
            get => defaultTextHash.Color;
            set => defaultTextHash.Color = value;
        }
        /// <summary>Gets the font that is used by renderer to create text.</summary>
        public SpriteFont DefaultFont
        {
            get => defaultTextHash.Font;
            set => defaultTextHash.Font = value;
        }

        private int LineSpacing => defaultTextHash.Font.LineSpacing;

        public RichTextRenderer(SpriteFont defaultFont) : this(defaultFont, null)
        {
        }

        public RichTextRenderer(SpriteFont defaultFont, BatchDrawOptions? options)
        {
            ArgumentNullException.ThrowIfNull(defaultFont);

            drawOptions = options ?? BatchDrawOptions.Default;
            defaultTextHash = new TextHash("", Color.White, defaultFont);

            _hashes = new List<IRendererHash>();
        }

        public RichTextRenderer(RichTextRenderer renderer)
        {
            ArgumentNullException.ThrowIfNull(renderer);

            drawOptions = renderer.drawOptions;
            defaultTextHash = new TextHash("", renderer.defaultTextHash.Color, renderer.defaultTextHash.Font);

            Text = renderer.Text;
            HorizontalTextLength = renderer.HorizontalTextLength;
            Opacity = renderer.Opacity;
            Orientation = renderer.Orientation;
            Alignment = renderer.Alignment;
            TextOffset = renderer.TextOffset;

            _hashes = new List<IRendererHash>(renderer._hashes);
        }

        public void AttachHash(IRendererHash hash)
        {
            _hashes.Add(hash);

            StringBuilder regexBuilder = new StringBuilder();
            for (int i = 0; i < _hashes.Count; i++)
            {
                IRendererHash item = _hashes[i];
                regexBuilder.Append("(?'").Append(i + 1).Append('\'').Append(item.Symbol);

                if (item.WrapsText)
                {
                    regexBuilder.Append(".{0,}").Append(item.Symbol);
                }

                regexBuilder.Append(")|");
            }

            regexBuilder.Remove(regexBuilder.Length - 1, 1); // remove the last '|' char

            _regexPattern = regexBuilder.ToString();
        }

        public void Render(RenderTarget2D target, Vector2 offset)
        {
            // right or middle orientations will cause a too large x offset that text won't appear, with maximum horizontal text length
            if (Orientation != TextOrientation.Left && HorizontalTextLength == int.MaxValue)
            {
                throw new ArgumentException("Maximum HorizontalTextLength should only be used with TextOrientation.Left");
            }

            DrawHelper.SetBeginBatch(target, drawOptions);

            List<(HashedTextPiece[] Parts, float LineWidth)> hashedLines = GetHashedLines();
            offset += TextOffset.ToVector2() + GetAlignmentOffset(new Point(target.Width, target.Height), TextSizeFromHashedLines(hashedLines));

            float spaceWidth = defaultTextHash.GetWidth(" ");
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

        private Point TextSizeFromHashedLines(List<(HashedTextPiece[] Parts, float LineWidth)> hashedLines)
        {
            if (HorizontalTextLength != int.MaxValue)
            {
                return new Point(HorizontalTextLength, LineSpacing * hashedLines.Count);
            }

            return new Point((int)hashedLines.Max(line => line.LineWidth), LineSpacing * hashedLines.Count);
        }

        private IRendererHash MatchedHash(Match match)
        {
            foreach (Group group in match.Groups)
            {
                if (group.Success && group.Name != "0")
                {
                    return _hashes[int.Parse(group.Name) - 1];
                }
            }

            throw new Exception();
        }

        private int[] AllNewLineIndexes(string text)
        {
            int count = 0;
            Span<int> indexes = stackalloc int[text.Length];

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n') indexes[count++] = i;
            }

            return indexes.Slice(0, count).ToArray();
        }

        private List<(HashedTextPiece[] Parts, float LineWidth)> GetHashedLines()
        {
            return SplitHorizontalLength(SplitToNewLines(SplitToHashes()));
        }

        private LinkedList<HashedTextPiece> SplitToHashes()
        {
            LinkedList<HashedTextPiece> hashedNodes = new LinkedList<HashedTextPiece>();
            if (_regexPattern == "")
            {
                hashedNodes.AddLast(new HashedTextPiece(Text, defaultTextHash));
                return hashedNodes;
            }

            int lastStart = 0;
            foreach (Match match in Regex.Matches(Text, _regexPattern, RegexOptions.Singleline))
            {
                if (lastStart < match.Index)
                {
                    hashedNodes.AddLast(new HashedTextPiece(Text.Substring(lastStart, match.Index - lastStart), defaultTextHash));
                }

                if (match.Length > 0)
                {
                    int index = match.Index;
                    int length = match.Length;

                    IRendererHash matchedHash = MatchedHash(match);
                    if (matchedHash.WrapsText)
                    {
                        index += matchedHash.Symbol.Length;
                        length -= matchedHash.Symbol.Length * 2;
                    }

                    hashedNodes.AddLast(new HashedTextPiece(Text.Substring(index, length), matchedHash));
                }

                lastStart = match.Index + match.Length;
            }

            if (lastStart < Text.Length)
            {
                hashedNodes.AddLast(new HashedTextPiece(Text.Substring(lastStart), defaultTextHash));
            }

            return hashedNodes;
        }

        private List<(string Word, IRendererHash Hash)[]> SplitToNewLines(LinkedList<HashedTextPiece> hashedNodes)
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

        private List<(HashedTextPiece[] Parts, float LineWidth)> SplitHorizontalLength(List<(string Word, IRendererHash Hash)[]> lineParts)
        {
            List<HashedTextPiece> currentLine = new List<HashedTextPiece>();
            List<(HashedTextPiece[] Parts, float LineWidth)> lines = new List<(HashedTextPiece[] Parts, float LineWidth)>();

            float spaceWidth = DefaultFont.MeasureString(" ").X;
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

        public Vector2 GetTextSize()
        {
            List<(string Word, IRendererHash Hash)[]> existingLines = SplitToNewLines(SplitToHashes());

            int lineCount = 1;
            float maxWidth = 0;

            float spaceWidth = DefaultFont.MeasureString(" ").X;
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

        public int GetVisibleCharCount()
        {
            return SplitToNewLines(SplitToHashes()).SelectMany(i => i).Sum(tuple => tuple.Hash.VisibleCharLength(tuple.Word) + 1) - 1;
        }

        public Vector2 ScreenCaretPosition(int caretPosition, Point? targetSize)
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

            float spaceWidth = defaultTextHash.GetWidth(" ");

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

        public int MouseCaretPosition(Vector2 mouseOnTarget, Point? targetSize)
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

            float wordSpacing = defaultTextHash.GetWidth(" ");
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

        private static int AllWordLengths(HashedTextPiece[] words)
        {
            return words.Sum(word => word.VisibleCharLength) 
                 + words.Length - 1; // for space characters
        }

        private class HashedTextPiece
        {
            public int VisibleCharLength => RendererHash.VisibleCharLength(Text);

            public string Text { get; set; }
            public IRendererHash RendererHash { get; set; }

            public HashedTextPiece(string text, IRendererHash hash)
            {
                Text = text;
                RendererHash = hash;
            }

            public float TextWidth()
            {
                return RendererHash.GetWidth(Text);
            }

            public float TextWidth(int startIndex, int length)
            {
                return RendererHash.GetWidth(Text.Substring(startIndex, length));
            }

            public void DrawHash(Vector2 position) 
            {
                RendererHash.Draw(Text, position);
            }
        }
    }
}
