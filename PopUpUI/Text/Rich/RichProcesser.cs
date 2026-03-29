using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PopUpUI.Text.Rich
{
    public abstract class RichProcesser : ITextProcesser
    {
        private int lineSpacing = -1;
        private float wordSpacing = -1;

        protected string _regexPattern = "";
        protected readonly List<IRendererHash> _hashes;
        protected readonly TextHash defaultTextHash;

        public int LineSpacing
        {
            get => lineSpacing >= 0 ? lineSpacing : defaultTextHash.Font.LineSpacing;
            set => lineSpacing = value;
        }

        public float WordSpacing
        {
            get => wordSpacing >= 0 ? wordSpacing : defaultTextHash.GetWidth(" ");
            set => wordSpacing = value;
        }

        public int HorizontalTextLength { get; set; } = int.MaxValue;
        public float FirstRowOffset { get; set; } = 0;
        public float Opacity { get; set; } = 1;
        public string Text { get; set; } = "";

        public TextAlignment Alignment { get; set; } = TextAlignment.TopLeft;
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

        public abstract ITextProcesserResult GetParseResult();

        public RichProcesser(Color defaultColor, SpriteFont defaultFont)
        {
            _hashes = new List<IRendererHash>();
            defaultTextHash = new TextHash("", defaultColor, defaultFont);
        }

        public void AttachHash(IRendererHash hash)
        {
            _hashes.Add(hash);

            CreateRegexPattern();
        }

        public void AttachHashes(IEnumerable<IRendererHash> hashes)
        {
            _hashes.AddRange(hashes);

            CreateRegexPattern();
        }

        private void CreateRegexPattern()
        {
            StringBuilder regexBuilder = new StringBuilder();
            for (int i = 0; i < _hashes.Count; i++)
            {
                IRendererHash item = _hashes[i];
                if (item.WrapsText)
                {
                    // example for symbol %
                    // %(?'2'[^%]*)%|
                    regexBuilder.Append($"{item.Symbol}(?'{i + 1}'[^{item.Symbol}]*){item.Symbol}|");
                }
                else
                {
                    // example for symbol :emoji:
                    // (?'2':emoji:)|
                    regexBuilder.Append($"(?'{i + 1}'{item.Symbol})|");
                }
            }

            regexBuilder.Remove(regexBuilder.Length - 1, 1); // remove the last '|' char

            _regexPattern = regexBuilder.ToString();
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

        protected List<ITextPiece[]> SplitToNewLines()
        {
            LinkedList<SingleHashedTextPiece> hashedNodes = SplitToHashes();

            List<ITextPiece[]> hashedWordLines = new List<ITextPiece[]>();
            List<ITextPiece> currentLine = new List<ITextPiece>();

            List<string> currentPieceParts = new List<string>();
            List<IRendererHash> currentPieceHashes = new List<IRendererHash>();

            for (var node = hashedNodes.First; node != null; node = node.Next)
            {
                int[] newLineIndex = ProcesserHelpers.AllNewLineIndexes(node.Value.Text);
                if (newLineIndex.Length == 0)
                {
                    AppendToCurrentLine(node.Value.Text, node.Value.RendererHash);
                    continue;
                }

                int lastStart = 0;
                foreach (var index in newLineIndex)
                {
                    AppendToCurrentLine(node.Value.Text.Substring(lastStart, index - lastStart), node.Value.RendererHash);
                    AddCurrentPieceToLine();

                    hashedWordLines.Add(currentLine.ToArray());

                    currentLine.Clear();
                    lastStart = index + 1;
                }

                AppendToCurrentLine(node.Value.Text.Substring(lastStart), node.Value.RendererHash);
            }

            AddCurrentPieceToLine();
            if (currentLine.Count != 0)
            {
                hashedWordLines.Add(currentLine.ToArray());
            }

            return hashedWordLines;

            void AppendToCurrentLine(string text, IRendererHash hash)
            {
                int start = 0;
                int end = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == ' ')
                    {
                        if (start != end)
                        {
                            currentPieceParts.Add(text[start..end]);
                            currentPieceHashes.Add(hash);

                            start = end;
                        }

                        start++;
                        AddCurrentPieceToLine();
                    }

                    end++;
                }

                if (start != end)
                {
                    currentPieceParts.Add(text[start..]);
                    currentPieceHashes.Add(hash);
                }
            }

            void AddCurrentPieceToLine()
            {
                if (currentPieceParts.Count == 0)
                {
                    return;
                }

                if (currentPieceParts.Count == 1)
                {
                    currentLine.Add(new SingleHashedTextPiece(currentPieceParts[0], currentPieceHashes[0]));
                }
                else
                {
                    currentLine.Add(new MultiHashedTextPiece(currentPieceParts.ToArray(), currentPieceHashes.ToArray()));
                }

                currentPieceParts.Clear();
                currentPieceHashes.Clear();
            }
        }

        private LinkedList<SingleHashedTextPiece> SplitToHashes()
        {
            LinkedList<SingleHashedTextPiece> hashedNodes = new LinkedList<SingleHashedTextPiece>();
            if (_regexPattern == "")
            {
                hashedNodes.AddLast(new SingleHashedTextPiece(Text, defaultTextHash));
                return hashedNodes;
            }

            int lastStart = 0;
            foreach (Match match in Regex.Matches(Text, _regexPattern, RegexOptions.Singleline))
            {
                if (lastStart < match.Index)
                {
                    hashedNodes.AddLast(new SingleHashedTextPiece(Text.Substring(lastStart, match.Index - lastStart), defaultTextHash));
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

                    hashedNodes.AddLast(new SingleHashedTextPiece(Text.Substring(index, length), matchedHash));
                }

                lastStart = match.Index + match.Length;
            }

            if (lastStart < Text.Length)
            {
                hashedNodes.AddLast(new SingleHashedTextPiece(Text.Substring(lastStart), defaultTextHash));
            }

            return hashedNodes;
        }
    }
}
