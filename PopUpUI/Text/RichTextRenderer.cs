using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

#nullable enable

namespace PopUpUI.Text
{
    /// <summary>Presents an object that can render text on <see cref="RenderTarget2D"/> in a more advanced way than <see cref="TextRenderer"/>.</summary>
    public class RichTextRenderer : TextRenderer
    {
        private ColorHash defaultColorHash = new ColorHash("+*^/", "/^*+", Color.White);

        /// <summary>Gets or sets a space length at the first line of text render. Default value is 0.</summary>
        public int SpaceLengthAtBeginning { get; set; } = 0;
        /// <summary>Gets or sets the defualt color of the text which is the part that is not hashed with any <see cref="ColorHash"/> in text.</summary>
        public override Color TextColor 
        {
            get => defaultColorHash.Color;
            set => defaultColorHash = new ColorHash(defaultColorHash.StartHash, defaultColorHash.EndHash, value);
        }

        /// <summary>Gets the color hashes this renderer contains.</summary>
        public IList<ColorHash> ColorHashes { get; }
        /// <summary>Gets the icon hashes this renderer contains.</summary>
        public IList<IconHash> IconHashes { get; }

        /// <summary>Initializes a new instance of <see cref="RichTextRenderer"/> class with properties from given <paramref name="renderer"/>.</summary>
        /// <param name="renderer">A <see cref="RichTextRenderer"/> instance to copy properties from.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RichTextRenderer(RichTextRenderer renderer) : base(renderer)
        {
            // TextColor is already set in base class
            SpaceLengthAtBeginning = renderer.SpaceLengthAtBeginning;

            ColorHashes = renderer.ColorHashes;
            IconHashes = renderer.IconHashes;
        }

        /// <summary>Initializes a new instance of <see cref="RichTextRenderer"/> class with properties from given <paramref name="renderer"/>.</summary>
        /// <param name="renderer">A <see cref="TextRenderer"/> instance to copy properties from.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RichTextRenderer(TextRenderer renderer) : base(renderer)
        {
            ColorHashes = new HashCollection<ColorHash>(this);
            IconHashes = new HashCollection<IconHash>(this);
        }

        /// <summary>Initializes a new instance of <see cref="RichTextRenderer"/> class with given <paramref name="font"/> and <paramref name="options"/> to use while drawing.</summary>
        /// <param name="font"><see cref="SpriteFont"/> that will be used while rendering text.</param>
        /// <param name="options"><see cref="BatchDrawOptions"/> that will be used while rendering text.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RichTextRenderer(SpriteFont font, BatchDrawOptions? options) : base(font, options)
        {
            ColorHashes = new HashCollection<ColorHash>(this);
            IconHashes = new HashCollection<IconHash>(this);
        }

        /// <summary>Initializes a new instance of <see cref="RichTextRenderer"/> class with given <paramref name="font"/> and default <see cref="BatchDrawOptions"/>.</summary>
        /// <param name="font"><see cref="SpriteFont"/> that will be used while rendering text.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RichTextRenderer(SpriteFont font) : this(font, null)
        {
        }

        public override void Render(RenderTarget2D target)
        {
            DrawHelper.SetRenderTarget(target);
            DrawHelper.BeginBatch(drawOptions);

            string modifiedTextWithHash = GetModifiedTextWithHash(GetModifiedText());
            Point textSize = GetTextSize();
            
            bool isFirstLine = true;
            float yOffset = GetYOffsetFromAlignment(target.Bounds.Size, textSize);
            switch (Orientation)
            {
                case TextOrientation.Left:
                case TextOrientation.Middle:
                case TextOrientation.Right:

                    foreach (var line in modifiedTextWithHash.Split('\n'))
                    {
                        float xOffset = GetXOffsetFromOrientation(line, isFirstLine) + GetXOffsetFromAlignment(target.Bounds.Size, textSize);
                        IEnumerable<(string, int, ColorHash)> differentColorParts = GetDifferentColorParts(line);

                        foreach (var (part, _, colorHash) in differentColorParts) // seperating line into different color parts
                        {
                            int startPosition = 0;
                            IEnumerable<(int, IconHash)> iconHashPositions = GetIconHashPositions(part);

                            foreach (var (Index, iconHash) in iconHashPositions) // for all iconhashes part has, first string until iconhash is drawn, then the icon.
                            {
                                DrawHelper.Batch.DrawString(Font, part[startPosition..Index],
                                                             new Vector2(xOffset, yOffset) + TextOffset.ToVector2(),
                                                             colorHash.Color * Opacity);
                                xOffset += GetLineLength(part[startPosition..Index]);

                                DrawHelper.Batch.Draw(iconHash.Icon,
                                                       new Rectangle(iconHash.Offset + new Vector2(xOffset, yOffset).ToPoint() + TextOffset, iconHash.Size),
                                                       Color.White);
                                xOffset += iconHash.Offset.X + iconHash.Size.X;

                                startPosition = Index + iconHash.Hash.Length;
                            }

                            // lastly we again draw a string in case there is some text after all iconhashes
                            DrawHelper.Batch.DrawString(Font, part[startPosition..], new Vector2(xOffset, yOffset) + TextOffset.ToVector2(), colorHash.Color * Opacity);
                            xOffset += GetLineLength(part[startPosition..]);
                        }

                        // line spacing after line rendering is over
                        yOffset += Font.LineSpacing;

                        if (isFirstLine) isFirstLine = false;
                    }

                    break;
                case TextOrientation.FixedSize:

                    foreach (var line in modifiedTextWithHash.Split('\n')) // text is written line by line
                    {
                        float xOffset = GetXOffsetFromOrientation(line, isFirstLine) + GetXOffsetFromAlignment(target.Bounds.Size, textSize);
                        IEnumerable<string> words = line.Split(' ');
                        float spaceLengthBetweenWords
                            = (HorizontalTextLength - (isFirstLine ? SpaceLengthAtBeginning : 0) - words.Select(GetLineLength).Sum()) / (words.Count() - 1);

                        foreach (var word in words) // draw line word by word because it is fixed size
                        {
                            var differentColorParts = GetDifferentColorParts(word);
                            foreach (var (part, _, colorHash) in differentColorParts) // drawing word with seperation color parts, in case it contains colorhash in it
                            {
                                if (ContainsIconHash(part, out IconHash foundHash))
                                {
                                    int startPosition = 0;
                                    IEnumerable<(int, IconHash)> iconHashPositions = GetIconHashPositions(part);

                                    foreach (var (Index, iconHash) in iconHashPositions) // for all iconhashes word has, first string until iconhash is drawn, then the icon.
                                    {
                                        DrawHelper.Batch.DrawString(Font, part[startPosition..Index],
                                                                     new Vector2(xOffset, yOffset) + TextOffset.ToVector2(),
                                                                     colorHash.Color * Opacity);
                                        xOffset += GetLineLength(part[startPosition..Index]);

                                        DrawHelper.Batch.Draw(iconHash.Icon,
                                                               new Rectangle(iconHash.Offset + new Vector2(xOffset, yOffset).ToPoint() + TextOffset, iconHash.Size),
                                                               Color.White);
                                        xOffset += iconHash.Offset.X + iconHash.Size.X;

                                        startPosition += Index + iconHash.Hash.Length;
                                    }

                                    // lastly we again draw a string in case there is some text after all iconhashes
                                    DrawHelper.Batch.DrawString(Font, part[startPosition..], new Vector2(xOffset, yOffset) + TextOffset.ToVector2(), colorHash.Color * Opacity);
                                    xOffset += GetLineLength(part[startPosition..]);
                                }
                                else // if it doesn't contain any icon hash, we just draw the part itself
                                {
                                    DrawHelper.Batch.DrawString(Font, part, new Vector2(xOffset, yOffset) + TextOffset.ToVector2(), colorHash.Color * Opacity);
                                    xOffset += GetLineLength(part);
                                }
                            }

                            // after word rendering is over, we add offset for the space.
                            // The reason it is here because words might contain icon or color hashes, and those are not counted as seperate words.
                            xOffset += spaceLengthBetweenWords;
                        }

                        // line spacing after line rendering is over
                        yOffset += Font.LineSpacing;

                        if (isFirstLine) isFirstLine = false;
                    }

                    break;
            }

            DrawHelper.EndBatch();
        }

        public override Point GetTextSize()
        {
            string modifiedText = GetModifiedText();

            if (HorizontalTextLength != int.MaxValue)
            {
                return new Point(HorizontalTextLength, (modifiedText.Count(ch => ch == '\n') + 1) * Font.LineSpacing);
            }

            int maxWidth = 0;
            foreach (var line in modifiedText.Split('\n'))
            {
                int imageSizes = 0;
                string replacedLine = line;
                foreach (var iconHash in IconHashes)
                {
                    int count = line.AllIndexesOf(iconHash.Hash).Count();
                    while (count-- > 0) imageSizes += iconHash.Size.X + iconHash.Offset.X;

                    replacedLine = line.Replace(iconHash.Hash, "");
                }

                maxWidth = Font.MeasureString(replacedLine).ToPoint().X + imageSizes;
            }

            return new Point(maxWidth == 0 ? 1 : maxWidth, (modifiedText.AllIndexesOf("\n").Count() + 1) * Font.LineSpacing);
        }

        public override Vector2 GetCursorPosition(int atStartOfIndex, Point? targetSizeRenderIn)
        {
            string modifiedText = GetModifiedText();

            var lines = modifiedText.Split('\n');
            int lineIndexCursorIn = modifiedText.Take(atStartOfIndex).Count(ch => ch == '\n');

            string lineContainingCursor = lines[lineIndexCursorIn];

            float xPosition = GetXOffsetFromOrientation(lineContainingCursor, isFirstLine: lineIndexCursorIn == 0);
            switch (Orientation)
            {
                case TextOrientation.Left:
                case TextOrientation.Middle:
                case TextOrientation.Right:

                    try // this is an escape point when cursor hits the start of the text
                    {
                        xPosition += GetLineLength(lineContainingCursor[..(atStartOfIndex - lines.Take(lineIndexCursorIn).Sum(str => str.Length) - lineIndexCursorIn)]);
                    }
                    catch // if cursor is at the beginning of the text, it is actually downgraded one line; but we increase index to put it in the correct place
                    {
                        lineIndexCursorIn += 1;
                    }

                    break;

                case TextOrientation.FixedSize:

                    string[] words = modifiedText.Split('\n')[lineIndexCursorIn].Split(' ');
                    float spaceLengthBetweenWords
                        = (HorizontalTextLength - (lineIndexCursorIn == 0 ? SpaceLengthAtBeginning : 0) - words.Select(GetLineLength).Sum()) / (words.Length - 1);

                    foreach (var word in words)
                    {
                        xPosition += GetLineLength(word) + spaceLengthBetweenWords;
                    }

                    xPosition -= spaceLengthBetweenWords;

                    break;
            }

            if (targetSizeRenderIn.HasValue)
            {
                Point textSize = GetTextSize();

                return new Vector2(xPosition, lineIndexCursorIn * Font.LineSpacing) +
                       TextOffset.ToVector2() +
                       new Vector2(GetXOffsetFromAlignment(targetSizeRenderIn.Value, textSize), GetYOffsetFromAlignment(targetSizeRenderIn.Value, textSize));
            }
            else
            {
                return new Vector2(xPosition, lineIndexCursorIn * Font.LineSpacing) +
                       TextOffset.ToVector2();
            }
        }

        public override int GetCursorPosition(Point positionOnText, Point? targetSizeRenderIn)
        {
            Point textSize = GetTextSize();

            positionOnText -= TextOffset;
            if (targetSizeRenderIn.HasValue)
            {
                positionOnText -= new Vector2(GetXOffsetFromAlignment(targetSizeRenderIn.Value, textSize),
                                              GetYOffsetFromAlignment(targetSizeRenderIn.Value, textSize)).ToPoint();
            }

            if (positionOnText.X >= textSize.X || positionOnText.Y >= textSize.Y) return Text.Length;

            string modifiedText = GetModifiedText();

            var lines = modifiedText.Split('\n');
            int lineIndex = positionOnText.Y / Font.LineSpacing;
            string lineCursorIn = lines[lineIndex];

            positionOnText.X -= (int)GetXOffsetFromOrientation(lineCursorIn, lineIndex == 0);

            // just an adjustment to be able to select next position if point already passed half of the character
            int alignAdjustment = Font.LineSpacing / 5;
            for (int i = 0; i <= lineCursorIn.Length; i++)
            {
                if (Font.MeasureString(lineCursorIn[..i]).X - alignAdjustment > positionOnText.X)
                {
                    return i - 1 + lines.Take(lineIndex).Sum(str => str.Length) + lineIndex;
                }
            }

            return lineCursorIn.Length + lines.Take(lineIndex).Sum(str => str.Length) + lineIndex;
        }

        private float GetXOffsetFromOrientation(string line, bool isFirstLine)
        {
            return Orientation switch
            {
                TextOrientation.Left or TextOrientation.FixedSize => isFirstLine ? SpaceLengthAtBeginning : 0,
                TextOrientation.Middle => (HorizontalTextLength - GetLineLength(line)) / 2 + (isFirstLine ? SpaceLengthAtBeginning : 0),
                TextOrientation.Right => HorizontalTextLength - GetLineLength(line) + (isFirstLine ? -SpaceLengthAtBeginning : 0),
                _ => 0,
            };
        }

        // modifies the modifiedText to have line breaks if conditions met, which does NOT contain any color hash, and used to modifiedTextWithHash and for getting text size
        private string GetModifiedText()
        {
            if (Text.Length == 0)
            {
                return "";
            }

            StringBuilder newString = new StringBuilder();
            string[] words = Text.Split(' ');

            // get rid of color hashes that words contain
            for (int i = 0; i < words.Length; i++)
            {
                if (ContainsColorHash(words[i], out string hashRemovedWord))
                {
                    words[i] = hashRemovedWord;
                }
            }

            StringBuilder line = new StringBuilder();
            float lineLength = SpaceLengthAtBeginning;
            float spaceLength = Font.MeasureString(" ").X;
            foreach (var word in words)
            {
                float wordLength = GetLineLength(word);

                if (lineLength + wordLength + spaceLength <= HorizontalTextLength)
                {
                    line.Append(word + " ");
                    lineLength += wordLength + spaceLength;
                }
                else if (lineLength + wordLength <= HorizontalTextLength)
                {
                    line.Append(word);
                    lineLength += wordLength;
                }
                else
                {
                    if (line.Length == 0) throw new ArgumentException("HorizontalTextLength is not enough for the word: " + word + ". It must be bigger than " + wordLength);

                    newString.Append(line.ToString().TrimEnd() + '\n');
                    line.Clear();
                    line.Append(word + " ");
                    lineLength = wordLength + spaceLength;
                }
            }

            return newString.Append(line).ToString();
        }

        // modifies the modifiedTextWithHash to have line breaks and have hashes around the words correctly, used to write text to texture.
        private string GetModifiedTextWithHash(string modifiedText)
        {
            var parts = GetDifferentColorParts(Text).Where(tuple => tuple.ColorHash.Color != this.TextColor);
            List<(string Word, int MinIndex, ColorHash Hash)> wordsAndHashes = new();

            foreach (var (Part, PartStartIndex, ColorHash) in parts)
            {
                var words = Part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words) wordsAndHashes.Add((word, PartStartIndex, ColorHash));
            }

            string newString = modifiedText;
            foreach (var (Word, MinIndex, Hash) in wordsAndHashes)
            {
                int index = newString.IndexOf(Word, MinIndex);

                // replaces the found word with its hashed version
                newString = newString[0..index] + Hash.StartHash + Word + Hash.EndHash + newString[(index + Word.Length)..];
            }

            return newString;
        }

        private float GetLineLength(string line)
        {
            float length = 0;

            while (ContainsColorHash(line, out string hashRemovedPart)) line = hashRemovedPart;
            while (ContainsIconHash(line, out IconHash foundHash))
            {
                int count = Regex.Count(line, foundHash.Hash);
                line = line.Replace(foundHash.Hash, "");
                length += (foundHash.Offset.X + foundHash.Size.X) * count;
            }

            return length + Font.MeasureString(line).X;
        }

        // Divides the text to parts that have different color hashes
        // and returns these not-hashed strings with colorhash to render them with the color colorhash has
        private IEnumerable<(string Part, int PartStartIndex, ColorHash ColorHash)> GetDifferentColorParts(string line)
        {
            int startingPoint = 0;
            var orderedParts = GetColorHashPositions(line);

            foreach (var (StartIndex, Length, Hash) in orderedParts)
            {
                yield return (line[startingPoint..StartIndex], startingPoint, this.defaultColorHash);
                yield return (line.Substring(StartIndex + Hash.StartHash.Length, Length - Hash.StartHash.Length - Hash.EndHash.Length), StartIndex, Hash);

                startingPoint = StartIndex + Length;
            }

            yield return (line[startingPoint..], startingPoint, this.defaultColorHash);
        }

        // Finds the hash positions that appears in the line
        private IEnumerable<(int StartIndex, int Length, ColorHash Hash)> GetColorHashPositions(string line)
        {
            List<(int StartIndex, int Length, ColorHash Hash)> hashedParts = new();

            foreach (var hash in ColorHashes)
            {
                // to not find the same end hash if same hash was used more than one time
                int lastEndHashIndex = 0;

                foreach (var startIndex in line.AllIndexesOf(hash.StartHash))
                {
                    int endIndex = line.IndexOf(hash.EndHash, lastEndHashIndex);
                    if (endIndex == -1) continue;

                    lastEndHashIndex = endIndex + hash.EndHash.Length;

                    hashedParts.Add((startIndex, endIndex - startIndex + hash.EndHash.Length, hash));
                }
            }

            return hashedParts.OrderBy(tuple => tuple.StartIndex);
        }

        private IEnumerable<(int StartIndex, IconHash Hash)> GetIconHashPositions(string line)
        {
            List<(int StartIndex, IconHash Hash)> hashedParts = new();

            foreach (var hash in IconHashes)
            {
                foreach (var startIndex in line.AllIndexesOf(hash.Hash))
                {
                    hashedParts.Add((startIndex, hash));
                }
            }

            return hashedParts.OrderBy(tuple => tuple.StartIndex);
        }

        // checks if word contains any hashes and removes them to use in ModifyTheText()
        private bool ContainsColorHash(string word, out string hashRemovedWord)
        {
            foreach (var hash in ColorHashes)
            {
                bool isStarted = word.Contains(hash.StartHash);
                bool isEnded = word.Contains(hash.EndHash);
                if (!(isStarted || isEnded)) continue;

                hashRemovedWord = word;

                if (isStarted) hashRemovedWord = hashRemovedWord.Replace(hash.StartHash, "");
                if (isEnded) hashRemovedWord = hashRemovedWord.Replace(hash.EndHash, "");

                return true;
            }

            hashRemovedWord = "";
            return false;
        }

        private bool ContainsIconHash(string word, out IconHash foundHash)
        {
            foreach (var hash in IconHashes)
            {
                if (word.Contains(hash.Hash))
                {
                    foundHash = hash;
                    return true;
                }
            }

            foundHash = default;
            return false;
        }
    }
}
