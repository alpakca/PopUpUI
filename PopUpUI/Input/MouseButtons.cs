using System;

namespace PopUpUI.Input
{
    /// <summary>Presents the buttons of a mouse.</summary>
    [Flags]
    public enum MouseButtons
    {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = 4,
        XButton1 = 8,
        XButton2 = 16,
        Any = Left | Right | Middle | XButton1 | XButton2
    }
}
