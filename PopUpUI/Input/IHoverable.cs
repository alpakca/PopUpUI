using Microsoft.Xna.Framework;
using PopUpUI.Elements;
using System;

namespace PopUpUI.Input
{
    /// <summary>Defines a mechanism for <see cref="PopUpElement"/> for achieving hovering operations.</summary>
    public interface IHoverable
    {
        /// <summary>Gets the current hover status of element.</summary>
        public bool IsHovered { get; }
        public IHoverablePacket HoverablePacket { get; }

        /// <summary>Checks if element can be hovered based on position of mouse.</summary>
        /// <param name="point">The mouse position inside <see cref="PopUpWindow"/> element is in.</param>
        /// <returns>Returns <see langword="true"/> if element can be hovered, else <see langword="false"/>.</returns>
        public bool CanBeHovered(Point point);
        /// <summary>Called while element is hovered.</summary>
        /// <param name="e">A <see cref="PopUpWindowEventArgs"/> object that contains event data.</param>
        public void OnHovering(PopUpWindowEventArgs e);
        /// <summary>Called when hover status of element has changed, started or ended.</summary>
        /// <param name="e">A <see cref="PopUpWindowEventArgs"/> object that contains event data.</param>
        /// <param name="isHovered">The new value for the hover.</param>
        public void OnHoverStateChange(PopUpWindowEventArgs e, bool isHovered);
    }
}
