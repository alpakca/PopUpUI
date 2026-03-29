using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace PopUpUI.Text.Rich
{
    public class RichTextBuilder
    {
        private static readonly BuilderNode newLineNode = new BuilderNode(null!, null);

        private int _visibleLength;
        private int _totalCharLength;
        private readonly LinkedList<BuilderNode> _chunks;

        public int Length => _visibleLength;

        public RichTextBuilder()
        {
            _chunks = new LinkedList<BuilderNode>();
        }

        public RichTextBuilder(string text) : this()
        {
            Append(text);
        }

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

        public RichTextBuilder Append(string text)
        {
            InternalAppend(text, null);

            return this;
        }

        public RichTextBuilder Append(string text, IRendererHash hash)
        {
            InternalAppend(text, hash);

            return this;
        }

        public RichTextBuilder AppendLine(string text)
        {
            InternalAppend(text, null);
            AppendNewLineNode();

            return this;
        }

        public RichTextBuilder AppendLine(string text, IRendererHash hash)
        {
            InternalAppend(text, hash);
            AppendNewLineNode();

            return this;
        }

        private void LengthUpdateNodeAdded(string text, IRendererHash? hash)
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

        private void InternalAppend(string text, IRendererHash? hash)
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

        private void InternalAddBefore(string text, IRendererHash? hash, LinkedListNode<BuilderNode> node)
        {
            LengthUpdateNodeAdded(text, hash);
            _chunks.AddBefore(node, new BuilderNode(text, hash));
        }

        private void InternalAddAfter(string text, IRendererHash? hash, LinkedListNode<BuilderNode> node)
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

        public RichTextBuilder Insert(int index, string text)
        {
            return Insert(index, text, null!);
        }

        public RichTextBuilder Insert(int index, string text, IRendererHash hash)
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

        public RichTextBuilder Remove(int startIndex, int count)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(startIndex, 0);
            ArgumentOutOfRangeException.ThrowIfLessThan(count, 0);
            ArgumentOutOfRangeException.ThrowIfLessThan(startIndex - count, 0);

            (LinkedListNode<BuilderNode>? indexNode, int consumedCharCount) = GetNodeContainsIndex(startIndex);
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

        public RichTextBuilder Clear()
        {
            _totalCharLength = 0;
            _visibleLength = 0;
            _chunks.Clear();

            return this;
        }

        public override string ToString()
        {
            Span<char> chars = stackalloc char[_totalCharLength];

            int offset = 0;
            LinkedListNode<BuilderNode>? node = _chunks.First;
            for (; node != null; node = node.Next)
            {
                if (node.Value == newLineNode)
                {
                    chars[offset] = '\n';
                    offset++;

                    continue;
                }

                if (node.Value.WrapsText)
                {
                    node.Value.Hash!.Symbol.CopyTo(chars.Slice(offset));
                    offset += node.Value.Hash.Symbol.Length;
                }

                node.Value.Text.CopyTo(chars.Slice(offset));
                offset += node.Value.Text.Length;

                if (node.Value.WrapsText)
                {
                    node.Value.Hash!.Symbol.CopyTo(chars.Slice(offset));
                    offset += node.Value.Hash.Symbol.Length;
                }
            }

            return new string(chars);
        }

        private class BuilderNode
        {
            public string Text;
            public IRendererHash? Hash;

            public int Length => Hash != null ? Hash.VisibleCharLength(Text) : Text.Length;
            public bool WrapsText => Hash != null ? Hash.WrapsText : false;

            public BuilderNode(string text, IRendererHash? hash)
            {
                Text = text;
                Hash = hash;
            }
        }
    }
}
