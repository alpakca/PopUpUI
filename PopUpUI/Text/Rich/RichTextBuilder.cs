using System;
using System.Collections.Generic;

#nullable enable

namespace PopUpUI.Text.Rich
{
    /// <summary>Represents a builder that can manipulate and build text for <see cref="RichProcesser"/>s.</summary>
    public class RichTextBuilder
    {
        private static readonly BuilderNode newLineNode = new BuilderNode(Environment.NewLine, null);

        private int _visibleLength;
        private int _totalCharLength;
        private readonly LinkedList<BuilderNode> _chunks;

        /// <summary>Gets the number of characters builder contains, including symbols. 
        /// It is also the length of the string builder would create.</summary>
        public int TotalCharLength => _totalCharLength;
        /// <summary>Gets the number of visible characters builder contains.</summary>
        public int VisibleLength => _visibleLength;

        /// <summary>Initialzies a new instance of <see cref="RichTextBuilder"/> class.</summary>
        public RichTextBuilder()
        {
            _chunks = new LinkedList<BuilderNode>();
        }

        /// <summary>Gets the visible character at given index.</summary>
        /// <param name="index">The index of character to get.</param>
        /// <returns>The character at given index position of visible characters.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public char this[int index]
        {
            get
            {
                if (index < 0 || index >= _visibleLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                (LinkedListNode<BuilderNode>? indexNode, int consumedCharCount) = GetNodeContainsIndex(index);

                return indexNode!.Value.Text[index - consumedCharCount];
            }
        }

        public string this[Range range]
        {
            get
            {
                return ToString(range.Start.IsFromEnd ? _visibleLength - range.Start.Value : range.Start.Value,
                                range.End.IsFromEnd ? _visibleLength - range.End.Value : range.End.Value);
            }
        }

        /// <summary>Gets the hash that is associated with visible character at given index.</summary>
        /// <param name="index">The index of visible character to get its associated hash.</param>
        /// <returns>The associated hash of the visible character at given index if have any; otherwise <see langword="null"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public IProcesserHash? GetHashAtIndex(int index)
        {
            if (index < 0 || index >= _visibleLength)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            (LinkedListNode<BuilderNode>? indexNode, _) = GetNodeContainsIndex(index);

            return indexNode!.Value.Hash;
        }

        /// <summary>Appends a new text without any hash.</summary>
        /// <param name="text">The text to append.</param>
        /// <returns>A reference to this builder after appending.</returns>
        public RichTextBuilder Append(string text)
        {
            InternalAppend(text, null);

            return this;
        }

        /// <summary>Appends a new text with given hash.</summary>
        /// <param name="text">The text to append.</param>
        /// <param name="hash">The hash to associate with given text.</param>
        /// <returns>A reference to this builder after appending.</returns>
        public RichTextBuilder Append(string text, IProcesserHash? hash)
        {
            InternalAppend(text, hash);

            return this;
        }

        /// <summary>Appends a new line to builder.</summary>
        /// <returns>A reference to this builder after appending.</returns>
        public RichTextBuilder AppendLine()
        {
            AppendNewLineNode();

            return this;
        }

        /// <summary>Appends a new text and a new line without any hash.</summary>
        /// <param name="text">The text to append.</param>
        /// <returns>A reference to this builder after appending.</returns>
        public RichTextBuilder AppendLine(string text)
        {
            InternalAppend(text, null);
            AppendNewLineNode();

            return this;
        }

        /// <summary>Appends a new text and a new line with given hash.</summary>
        /// <param name="text">The text to append.</param>
        /// <param name="hash">The hash to associate with given text.</param>
        /// <returns>A reference to this builder after appending.</returns>
        public RichTextBuilder AppendLine(string text, IProcesserHash? hash)
        {
            InternalAppend(text, hash);
            AppendNewLineNode();

            return this;
        }

        private void LengthUpdateNodeAdded(string text, IProcesserHash? hash)
        {
            ArgumentNullException.ThrowIfNull(text, nameof(text));

            _totalCharLength += text.Length;
            if (hash != null)
            {
                _totalCharLength += hash.WrapsText ? hash.Symbol.Length * 2 : 0;
                _visibleLength += hash.VisibleCharLength(text);
            }
            else
            {
                _visibleLength += text.Length;
            }
        }

        private void LengthUpdateNodeRemoved(BuilderNode node)
        {
            _totalCharLength -= node.Text.Length;
            if (node.Hash != null)
            {
                _totalCharLength -= node.Hash.WrapsText ? node.Hash.Symbol.Length * 2 : 0;
                _visibleLength -= node.Hash.VisibleCharLength(node.Text);
            }
            else
            {
                _visibleLength -= node.Text.Length;
            }
        }

        private void InternalAppend(string text, IProcesserHash? hash)
        {
            LengthUpdateNodeAdded(text, hash);
            _chunks.AddLast(new BuilderNode(text, hash));
        }

        private void AppendNewLineNode()
        {
            _totalCharLength++;
            _visibleLength++;
            _chunks.AddLast(newLineNode);
        }

        private void InternalAddBefore(string text, IProcesserHash? hash, LinkedListNode<BuilderNode> node)
        {
            LengthUpdateNodeAdded(text, hash);
            _chunks.AddBefore(node, new BuilderNode(text, hash));
        }

        private void InternalAddAfter(string text, IProcesserHash? hash, LinkedListNode<BuilderNode> node)
        {
            LengthUpdateNodeAdded(text, hash);
            _chunks.AddAfter(node, new BuilderNode(text, hash));
        }

        private (LinkedListNode<BuilderNode>? Node, int ConsumedCharCount) GetNodeContainsIndex(int index)
        {
            if (index > _visibleLength)
            {
                throw new ArgumentException("index is not inside bounds of builder", nameof(index));
            }

            if (index == _visibleLength)
            {
                return (null, _visibleLength);
            }

            int consumedChars = 0;
            LinkedListNode<BuilderNode>? node = _chunks.First;

            while (node != null)
            {
                if (consumedChars + node.Value.Length > index)
                {
                    return (node, consumedChars);
                }

                consumedChars += node.Value.Length;
                node = node.Next;
            }

            return (_chunks.Last, consumedChars);
        }

        /// <summary>Inserts text without any hash at given <paramref name="index"/>.</summary>
        /// <param name="index">The index to insert given text.</param>
        /// <param name="text">The text to insert.</param>
        /// <returns>A reference to this builder after inserting.</returns>
        public RichTextBuilder Insert(int index, string text)
        {
            return Insert(index, text, null!);
        }

        /// <summary>Inserts text with given hash at given <paramref name="index"/>.</summary>
        /// <param name="index">The index to insert given text.</param>
        /// <param name="text">The text to insert.</param>
        /// <param name="hash">The hash to associate with given text.</param>
        /// <returns>A reference to this builder after inserting.</returns>
        public RichTextBuilder Insert(int index, string text, IProcesserHash? hash)
        {
            (LinkedListNode<BuilderNode>? indexNode, int consumedCharCount) = GetNodeContainsIndex(index);

            // it was last index of builder
            if (indexNode == null)
            {
                InternalAppend(text, hash);
                return this;
            }

            // inserting just before found node
            if (index - consumedCharCount == 0)
            {
                InternalAddBefore(text, hash, indexNode);
                return this;
            }

            LengthUpdateNodeRemoved(indexNode.Value);

            // split the node into two parts
            BuilderNode right = new BuilderNode(indexNode.Value.Text.Substring(index - consumedCharCount), indexNode.Value.Hash);
            indexNode.Value.Text = indexNode.Value.Text.Substring(0, index - consumedCharCount);

            LengthUpdateNodeAdded(indexNode.Value.Text, indexNode.Value.Hash);

            InternalAddAfter(right.Text, right.Hash, indexNode);
            InternalAddAfter(text, hash, indexNode);

            return this;
        }

        /// <summary>Removes number of visible characters of <paramref name="count"/> starting from <paramref name="startIndex"/>.</summary>
        /// <param name="startIndex">The index to start removing.</param>
        /// <param name="count">The number of visible characters to be removed.</param>
        /// <returns>A reference to this builder after removing.</returns>
        public RichTextBuilder Remove(int startIndex, int count)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(startIndex, 0);
            ArgumentOutOfRangeException.ThrowIfLessThan(count, 0);
            ArgumentOutOfRangeException.ThrowIfLessThan(startIndex - count, 0);

            (LinkedListNode<BuilderNode>? indexNode, _) = GetNodeContainsIndex(startIndex);
            indexNode ??= indexNode!.Previous;

            _visibleLength -= count;

            while (indexNode!.Value.Length < count)
            {
                count -= indexNode.Value.Length;
                LinkedListNode<BuilderNode> previous = indexNode.Previous!;

                LengthUpdateNodeRemoved(indexNode.Value);
                _chunks.Remove(indexNode);

                indexNode = previous;
            }

            if (count > 0)
            {
                if (indexNode.Value.Length == count)
                {
                    LengthUpdateNodeRemoved(indexNode.Value);
                    _chunks.Remove(indexNode);
                }
                else
                {
                    indexNode.Value.Text = indexNode.Value.Text[..^count];
                }

                _totalCharLength -= count;
            }

            return this;
        }

        /// <summary>Clears the content text builder contains.</summary>
        /// <returns>A reference to this builder after cleaning builder's content.</returns>
        public RichTextBuilder Clear()
        {
            _totalCharLength = 0;
            _visibleLength = 0;
            _chunks.Clear();

            return this;
        }

        /// <summary>Creates a new <see cref="RichTextBuilder"/> with given <paramref name="length"/> of 
        /// number of visible characters starting at <paramref name="startIndex"/>. </summary>
        /// <param name="startIndex">The index to start sub-building.</param>
        /// <param name="length">The number of visible characters to be sub-builded.</param>
        /// <returns>The created sub-builder.</returns>
        public RichTextBuilder Subbuilder(int startIndex, int length)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(startIndex, 0);
            ArgumentOutOfRangeException.ThrowIfLessThan(length, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(startIndex + length, _visibleLength);

            if (startIndex == _visibleLength || length == 0)
            {
                return new RichTextBuilder();
            }

            (LinkedListNode<BuilderNode>? node, int consumedCharCount) = GetNodeContainsIndex(startIndex);

            RichTextBuilder builder = new RichTextBuilder();
            while (length > 0)
            {
                int start = Math.Max(startIndex - consumedCharCount, 0);
                int count = Math.Clamp(length, 0, node!.Value.Text.Length - start);

                string text = node.Value.Text.Substring(start, count);
                builder.Append(text, node.Value.Hash);

                length -= count;
                consumedCharCount += count;

                node = node.Next;
            }

            return builder;
        }

        /// <summary>Transforms contents of this builder to a string.</summary>
        /// <returns>The created string.</returns>
        public override string ToString()
        {
            Span<char> chars = stackalloc char[_totalCharLength];

            int offset = 0;
            for (LinkedListNode<BuilderNode>? node = _chunks.First; node != null; node = node.Next)
            {
                if (node.Value.WrapsText)
                {
                    // symbol around text
                    node.Value.Hash!.Symbol.CopyTo(chars.Slice(offset));
                    offset += node.Value.Hash.Symbol.Length;

                    // actual text
                    node.Value.Text.CopyTo(chars.Slice(offset));
                    offset += node.Value.Text.Length;

                    // symbol around text
                    node.Value.Hash!.Symbol.CopyTo(chars.Slice(offset));
                    offset += node.Value.Hash.Symbol.Length;
                }
                else
                {
                    // actual text
                    node.Value.Text.CopyTo(chars.Slice(offset));
                    offset += node.Value.Text.Length;
                }
            }

            return new string(chars);
        }

        /// <summary>Transforms contents of this builder to a string starting 
        /// at <paramref name="startIndex"/> and for <paramref name="length"/> of visible characters..</summary>
        /// <param name="startIndex">Start index of substring.</param>
        /// <param name="length">The length of visible characters of substring.</param>
        /// <returns>The created string.</returns>
        public string ToString(int startIndex, int length)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(startIndex, 0);
            ArgumentOutOfRangeException.ThrowIfLessThan(length, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(startIndex + length, _visibleLength);

            if (startIndex == _visibleLength || length == 0)
            {
                return string.Empty;
            }

            // we can't just use length here because we don't know how many hashes there are and their hash lenghts
            Span<char> chars = stackalloc char[_totalCharLength - startIndex];

            int offset = 0;
            (LinkedListNode<BuilderNode>? node, int consumedCharCount) = GetNodeContainsIndex(startIndex);
            while (length > 0)
            {
                int start = Math.Max(startIndex - consumedCharCount, 0);
                int count = Math.Clamp(length, 0, node!.Value.Text.Length - start);

                if (node.Value.WrapsText)
                {
                    // symbol around text
                    node.Value.Hash!.Symbol.CopyTo(chars.Slice(offset));
                    offset += node.Value.Hash.Symbol.Length;

                    // actual text
                    node.Value.Text.Substring(start, count).CopyTo(chars.Slice(offset));
                    offset += count;

                    // symbol around text
                    node.Value.Hash!.Symbol.CopyTo(chars.Slice(offset));
                    offset += node.Value.Hash.Symbol.Length;
                }
                else
                {
                    // actual text
                    node.Value.Text.Substring(start, count).CopyTo(chars.Slice(offset));
                    offset += count;
                }

                length -= count;
                node = node.Next;
            }

            return new string(chars.Slice(0, offset));
        }

        private class BuilderNode
        {
            public string Text;
            public IProcesserHash? Hash;

            public int Length => Hash != null ? Hash.VisibleCharLength(Text) : Text.Length;
            public bool WrapsText => Hash != null ? Hash.WrapsText : false;

            public BuilderNode(string text, IProcesserHash? hash)
            {
                Text = text;
                Hash = hash;
            }
        }
    }
}
