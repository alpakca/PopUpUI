using Microsoft.Xna.Framework;
using PopUpUI.Elements;
using System;

namespace PopUpUI.Input
{
    /// <summary>Defines a mechanism for <see cref="PopUpElement"/> for receiving mouse wheel value changes.</summary>
    public interface IMouseWheelReceiver
    {
        /// <summary>Defines a criteria for receiving mouse wheel changes based on element itself.</summary>
        public Predicate<PopUpElement> DoNotReceiveWheelChangeIf { get; set; }
        /// <summary>Raised when mouse wheel value has changed.</summary>
        public event PopUpElementHandler MouseWheelChanged;
        /// <summary>Checks if element can receive mouse wheel change based on position of mouse.</summary>
        /// <param name="point">The mouse position inside <see cref="PopUpWindow"/> element is in.</param>
        /// <returns>Returns <see langword="true"/> if element can be receive mouse wheel change, else <see langword="false"/>.</returns>
        public bool CanReceiveWheelChangeInput(Point point);
        /// <summary>Called when mouse wheel value has changed.</summary>
        /// <param name="e">A <see cref="PopUpWindowEventArgs"/> object that contains event data.</param>
        public void OnMouseWheelChangeReceive(PopUpWindowEventArgs e);
    }
}
