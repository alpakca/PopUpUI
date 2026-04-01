using Microsoft.Xna.Framework;
using PopUpUI.Elements;

namespace PopUpUI.Input
{
    /// <summary>Defines a mechanism for <see cref="PopUpElement"/> for achieving holding operations.</summary>
    public interface IHoldable
    {
        /// <summary>Gets the current hold status of element.</summary>
        public bool IsHeld { get; }
        public HoldablePacket HoldPacket { get; }

        /// <summary>Checks if element can be held based on position of mouse.</summary>
        /// <param name="point">The mouse position inside <see cref="PopUpWindow"/> element is in.</param>
        /// <returns>Returns <see langword="true"/> if element can be held, else <see langword="false"/>.</returns>
        public bool CanBeHeld(Point point);
        /*
        /// <summary>Called while element can be held.</summary>
        /// <param name="e">A <see cref="PopUpWindowEventArgs"/> object that contains event data.</param>
        /// <param name="isHeld">The value of holding which determined by if prefered mouse button is down.</param>
        public void HoldStatusUpdate(PopUpWindowEventArgs e, bool isHeld);*/

        public void OnHolding(PopUpWindowEventArgs e);
        public void OnHoldStateChange(PopUpWindowEventArgs e, bool startedHolding);
    }
}
