using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PopUpUI.Text
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

        protected LinkedList<HashedTextPiece> SplitToHashes()
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
    }
}
