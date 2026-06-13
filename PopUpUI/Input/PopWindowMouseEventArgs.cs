using Microsoft.Xna.Framework;

namespace PopUpUI.Input
{
    public class PopWindowMouseEventArgs
    {
        private readonly Point mousePosition;

        private readonly MouseButtons firstTimePressed;
        private readonly MouseButtons currentlyPressed;
        private readonly MouseButtons firstTimeReleased;

        public Point MousePosition => mousePosition;

        public MouseButtonArgs FirstTimeButtons => new MouseButtonArgs(firstTimePressed);
        public MouseButtonArgs CurrentButtons => new MouseButtonArgs(currentlyPressed);
        public MouseButtonArgs ReleasedButtons => new MouseButtonArgs(firstTimeReleased);

        public PopWindowMouseEventArgs(MouseButtons firstTimePressed, MouseButtons currentlyPressed, MouseButtons firstTimeReleased, Point mousePosition)
        {
            this.mousePosition = mousePosition;

            this.firstTimePressed = firstTimePressed;
            this.currentlyPressed = currentlyPressed;
            this.firstTimeReleased = firstTimeReleased;
        }

        public PopWindowMouseEventArgs WithMousePosition(Point mousePosition)
        {
            return new PopWindowMouseEventArgs(this.firstTimePressed, this.currentlyPressed, this.firstTimeReleased, mousePosition);
        }
    }

    public readonly struct MouseButtonArgs
    {
        private readonly MouseButtons _buttons;

        public bool HasLeft => _buttons.HasFlag(MouseButtons.Left);
        public bool HasRight => _buttons.HasFlag(MouseButtons.Right);
        public bool HasMiddle => _buttons.HasFlag(MouseButtons.Middle);
        public bool HasXButton1 => _buttons.HasFlag(MouseButtons.XButton1);
        public bool HasXButton2 => _buttons.HasFlag(MouseButtons.XButton2);

        public bool HasAny => _buttons != MouseButtons.None;
        public bool HasAll => _buttons.HasFlag(MouseButtons.Any);

        public MouseButtonArgs(MouseButtons pressed)
        {
            _buttons = pressed;
        }

        public bool OverlapsWith(MouseButtons button)
        {
            return (_buttons & button) != MouseButtons.None;
        }

        public override string ToString()
        {
            return _buttons.ToString();
        }
    }
}
