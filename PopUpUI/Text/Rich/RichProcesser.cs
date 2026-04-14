using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PopUpUI.Text.Rich
{
    /// <summary>Represents a rich processer, that may contain <see cref="IProcesserHash"/>es.</summary>
    public abstract class RichProcesser : ITextProcesser
    {
        private string _regexPattern = "";
        private readonly List<IProcesserHash> _hashes;
        private readonly TextHash defaultTextHash;

        public int LineSpacing { get; set; }
        public float WordSpacing { get; set; }
        
        public int HorizontalTextLength { get; set; } = int.MaxValue;
        public float FirstRowOffset { get; set; } = 0;
        public string Text { get; set; } = "";

        public TextAlignment Alignment { get; set; } = TextAlignment.TopLeft;

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
            _hashes = new List<IProcesserHash>();
            defaultTextHash = new TextHash("", defaultColor, defaultFont);

            LineSpacing = defaultFont.LineSpacing;
            WordSpacing = defaultFont.MeasureString(" ").X;
        }

        public RichProcesser(RichProcesser processer) : this(processer.defaultTextHash.Color, processer.defaultTextHash.Font)
        {
            LineSpacing = processer.LineSpacing;
            WordSpacing = processer.WordSpacing;
            HorizontalTextLength = processer.HorizontalTextLength;
            FirstRowOffset = processer.FirstRowOffset;
            Text = processer.Text;
            Alignment = processer.Alignment;
        }

        /// <summary>Attaches a new <see cref="IProcesserHash"/> to be used while parsing.</summary>
        /// <param name="hash">The hash to be used in parsing text.</param>
        public void AttachHash(IProcesserHash hash)
        {
            _hashes.Add(hash);

            CreateRegexPattern();
        }

        /// <summary>Attaches a group of <see cref="IProcesserHash"/>es to be used while parsing.</summary>
        /// <param name="hashes">Hashes to be used in parsing text.</param>
        public void AttachHashes(IEnumerable<IProcesserHash> hashes)
        {
            _hashes.AddRange(hashes);

            CreateRegexPattern();
        }

        private void CreateRegexPattern()
        {
            StringBuilder regexBuilder = new StringBuilder();
            for (int i = 0; i < _hashes.Count; i++)
            {
                IProcesserHash item = _hashes[i];
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

        private IProcesserHash MatchedHash(Match match)
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

        protected LinkedList<SingleHashedTextPiece> SplitToHashes()
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

                    IProcesserHash matchedHash = MatchedHash(match);
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
