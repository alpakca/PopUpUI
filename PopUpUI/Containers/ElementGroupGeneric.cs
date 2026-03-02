using PopUpUI.Elements;
using System.Collections.Generic;
using System.Diagnostics;

#nullable enable

namespace PopUpUI.Containers
{
    /// <summary>Represents a group of <see cref="PopUpElement"/>s with a leader element and supports operations to be applied in the group.</summary>
    /// <typeparam name="TLeader">Type of the leader element.</typeparam>
    [DebuggerDisplay("LeaderType: {Leader.GetType().Name}, Count = {Count,nq}{Name.Length > 0 ? \", Name: \" + Name : System.String.Empty,nq}")]
    public class ElementGroup<TLeader> : ElementGroup where TLeader : PopUpElement
    {
        /// <summary>The leader element of the group.</summary>
        public TLeader Leader { get; }

        /// <summary>Initializes a new instance of <see cref="ElementGroup{TLeader}"/> class with its leader.</summary>
        /// <param name="leader">The leader of the group.</param>
        public ElementGroup(TLeader leader) : base()
        {
            Leader = leader;
            Add(leader);
        }

        /// <summary>Initializes a new instance of <see cref="ElementGroup{TLeader}"/> class with its leader and a comparer for Z-Indexes.</summary>
        /// <param name="leader">The leader of the group.</param>
        /// <param name="zIndexComparer">Comparer for the Z-Indexes.</param>
        public ElementGroup(TLeader leader, IComparer<int>? zIndexComparer) : this(leader, [], zIndexComparer)
        {
        }

        /// <summary>Initializes a new instance of <see cref="ElementGroup{TLeader}"/> class with its leader and elements copied from <paramref name="collection"/>.</summary>
        /// <param name="leader">The leader of the group.</param>
        /// <param name="collection">Elements to copy from.</param>
        public ElementGroup(TLeader leader, IEnumerable<PopUpElement> collection) : this(leader, collection, null)
        {
        }

        /// <summary>Initializes a new instance of <see cref="ElementGroup{TLeader}"/> class with its leader, 
        /// elements copied from <paramref name="collection"/> and a comparer for Z-Indexes.</summary>
        /// <param name="leader">The leader of the group.</param>
        /// <param name="collection">Elements to copy from.</param>
        /// <param name="zIndexComparer">Comparer for the Z-Indexes.</param>
        public ElementGroup(TLeader leader, IEnumerable<PopUpElement> collection, IComparer<int>? zIndexComparer) : base(collection, zIndexComparer)
        {
            Leader = leader;
            Add(leader);
        }
    }
}
