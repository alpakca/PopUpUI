using System;

namespace PopUpUI.Elements
{
    /// <summary>
    /// Presents the directions of a movement operation.
    /// </summary>
    [Flags]
    public enum MovementDirection
    {
        /// <summary>Can only move horizontally; left to right or vice versa.</summary>
        Horizontal = 1,
        /// <summary>Can only move vertically; top to bottom or vice versa.</summary>
        Vertical = 2,

        /// <summary>Can move vertically or horizontally, to any direction.</summary>
        Any = Horizontal | Vertical
    }
}
