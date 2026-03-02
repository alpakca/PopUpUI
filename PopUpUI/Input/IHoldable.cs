using Microsoft.Xna.Framework;
using PopUpUI.Elements;
using System;

namespace PopUpUI.Input
{
    /// <summary>Defines a mechanism for <see cref="PopUpElement"/> for achieving holding operations.</summary>
    public interface IHoldable
    {
        /// <summary>Gets the current hold status of element.</summary>
        public bool IsHeld { get; }
        public MouseButtons ButtonsToHold { get; set; }
        /// <summary>Defines a criteria for holding based on element itself.</summary>
        public Predicate<PopUpElement> DoNotHoldIf { get; set; }
        /// <summary>Raised while element is held.</summary>
        public event PopUpElementHandler Held;
        /// <summary>Raised when hold status of element has changed, started or ended. Bool gets if its new status is held.</summary>
        public event PopUpElementHandler<bool> HoldStateChange;
        /// <summary>Checks if element can be held based on position of mouse.</summary>
        /// <param name="point">The mouse position inside <see cref="PopUpWindow"/> element is in.</param>
        /// <returns>Returns <see langword="true"/> if element can be held, else <see langword="false"/>.</returns>
        public bool CanBeHeld(Point point);
        /// <summary>Called while element can be held.</summary>
        /// <param name="e">A <see cref="PopUpWindowEventArgs"/> object that contains event data.</param>
        /// <param name="isHeld">The value of holding which determined by if prefered mouse button is down.</param>
        public void HoldStatusUpdate(PopUpWindowEventArgs e, bool isHeld);
    }
}
