using Microsoft.Xna.Framework;
using PopUpUI.Elements;

namespace PopUpUI.Input
{
    /// <summary>Provides information for <see cref="Surface.PopUpWindow"/> and <see cref="PopUpElement"/> handlers.</summary>
    /// <param name="PressedButtons">Gets the buttons that are currently being pressed.</param>
    /// <param name="NewlyPressedButtons">Gets the buttons that just has been pressed.</param>
    /// <param name="MousePoint">Position of the mouse on screen.</param>
    /// <param name="MouseWheelChange">Mouse wheel's value change.</param>
    public record class PopUpWindowEventArgs(MouseButtons PressedButtons, MouseButtons NewlyPressedButtons, Point MousePoint, int MouseWheelChange)
    {
        public static PopUpWindowEventArgs Empty { get; } = new PopUpWindowEventArgs(MouseButtons.None, MouseButtons.None, new Point(), 0);
    }
}
