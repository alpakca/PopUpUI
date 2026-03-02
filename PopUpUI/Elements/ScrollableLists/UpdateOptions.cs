using System;

namespace PopUpUI.Elements
{
    /// <summary>Presents the options to update the <see cref="ScrollableList"/>.</summary>
    [Flags]
    public enum ScrollableListUpdateOptions
    {
        /// <summary>The default option for the <see cref="ScrollableList"/>.</summary>
        Default = Scrolled | NodeListChanged | AnyNodeHadEvent,
        /// <summary>When list have scrolled.</summary>
        Scrolled = 1,
        /// <summary>When <see cref="ScrollableList.Nodes"/>'s nodes have changed.</summary>
        NodeListChanged = 2,
        /// <summary>When any node has selected or held. For this option to work, nodes needed to be any of <see cref="ISelectable"/>, <see cref="IHoldable"/> or <see cref="IHoverable"/>.</summary>
        AnyNodeHadEvent = 4,
        /// <summary>Updates the list once every frame. If the visuals of the nodes are constantly updated, this is the recommended option to see all updated visuals.</summary>
        EveryFrame = 8
    }
}
