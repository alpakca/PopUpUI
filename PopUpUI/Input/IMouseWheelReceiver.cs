using Microsoft.Xna.Framework;
using PopUpUI.Elements;

namespace PopUpUI.Input
{
    /// <summary>Defines a mechanism for <see cref="PopUpElement"/> for receiving mouse wheel value changes.</summary>
    public interface IMouseWheelReceiver
    {
        public MouseWheelReceiverPacket MouseWheelReceiverPacket { get; }

        /// <summary>Checks if element can receive mouse wheel change based on position of mouse.</summary>
        /// <param name="point">The mouse position inside <see cref="PopUpWindow"/> element is in.</param>
        /// <returns>Returns <see langword="true"/> if element can be receive mouse wheel change, else <see langword="false"/>.</returns>
        public bool CanReceiveWheelChangeInput(Point point);
        /// <summary>Called when mouse vertical scroll value has changed.</summary>
        /// <param name="e">A <see cref="PopWindowMouseEventArgs"/> object that contains event data.</param>
        public void OnMouseVerticalScrollChangeReceive(PopWindowMouseEventArgs e, int change);
        /// <summary>Called when mouse horizontal scroll value has changed.</summary>
        /// <param name="e">A <see cref="PopWindowMouseEventArgs"/> object that contains event data.</param>
        public void OnMouseHorizontalScrollChangeReceive(PopWindowMouseEventArgs e, int change);
    }
}
