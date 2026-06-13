using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PopUpUI.Input;

namespace PopUpUI.Toolkit
{
    public delegate void InputManagerHandler(PopWindowMouseEventArgs e);
    public delegate void InputManagerHandler<TArgs>(PopWindowMouseEventArgs e, TArgs args);

    public static class InputManager
    {
        private static KeyboardState previousKeyboard;
        private static MouseState previousMouse;
        private static MouseButtons previousCurrentButtons;

        private static KeyboardState newKeyboard;
        private static MouseState newMouse;
        private static MouseButtons newCurrentButtons;

        private static PopWindowMouseEventArgs lastArgs;

        public static event InputManagerHandler MouseButtonStateChange;
        public static event InputManagerHandler MousePositionChange;
        public static event InputManagerHandler<int> MouseVerticalScrollChange;
        public static event InputManagerHandler<int> MouseHorizontalScrollChange;
        public static event InputManagerHandler<Keys> KeyDown;
        public static event InputManagerHandler<Keys> KeyUp;
        public static event InputManagerHandler<char> TextInput;
        public static event InputManagerHandler<FileDropEventArgs> FileDrop;

        public static bool IsCapsLockOn => newKeyboard.CapsLock;
        public static bool IsNumLockOn => newKeyboard.NumLock;

        internal static void Initialize(GameWindow window)
        {
            window.KeyDown += (_, e) => KeyDown?.Invoke(lastArgs, e.Key);
            window.KeyUp += (_, e) => KeyUp?.Invoke(lastArgs, e.Key);
            window.TextInput += (_, e) => TextInput?.Invoke(lastArgs, e.Character);
            window.FileDrop += (_, e) => FileDrop?.Invoke(lastArgs, e);
        }

        internal static void Update()
        {
            previousKeyboard = newKeyboard;
            previousMouse = newMouse;

            newKeyboard = Keyboard.GetState();
            newMouse = Mouse.GetState();

            previousCurrentButtons = newCurrentButtons;
            newCurrentButtons = GetCurrentButtons();

            lastArgs = new PopWindowMouseEventArgs(GetFirstTimeButtons(), newCurrentButtons, GetReleasedButtons(), newMouse.Position);

            if (newCurrentButtons != previousCurrentButtons)
            {
                MouseButtonStateChange?.Invoke(lastArgs);
            }

            if (newMouse.Position != previousMouse.Position)
            {
                MousePositionChange?.Invoke(lastArgs);
            }

            if (newMouse.ScrollWheelValue != previousMouse.ScrollWheelValue)
            {
                MouseVerticalScrollChange?.Invoke(lastArgs, newMouse.ScrollWheelValue - previousMouse.ScrollWheelValue);
            }

            if (newMouse.HorizontalScrollWheelValue != previousMouse.HorizontalScrollWheelValue)
            {
                MouseHorizontalScrollChange?.Invoke(lastArgs, newMouse.HorizontalScrollWheelValue - previousMouse.HorizontalScrollWheelValue);
            }
        }

        private static MouseButtons GetFirstTimeButtons()
        {
            MouseButtons buttons = MouseButtons.None;

            if (newMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton != ButtonState.Pressed) buttons |= MouseButtons.Left;
            if (newMouse.RightButton == ButtonState.Pressed && previousMouse.RightButton != ButtonState.Pressed) buttons |= MouseButtons.Right;
            if (newMouse.MiddleButton == ButtonState.Pressed && previousMouse.MiddleButton != ButtonState.Pressed) buttons |= MouseButtons.Middle;
            if (newMouse.XButton1 == ButtonState.Pressed && previousMouse.XButton1 != ButtonState.Pressed) buttons |= MouseButtons.XButton1;
            if (newMouse.XButton2 == ButtonState.Pressed && previousMouse.XButton2 != ButtonState.Pressed) buttons |= MouseButtons.XButton2;

            return buttons;
        }

        private static MouseButtons GetCurrentButtons()
        {
            MouseButtons buttons = MouseButtons.None;

            if (newMouse.LeftButton == ButtonState.Pressed) buttons |= MouseButtons.Left;
            if (newMouse.RightButton == ButtonState.Pressed) buttons |= MouseButtons.Right;
            if (newMouse.MiddleButton == ButtonState.Pressed) buttons |= MouseButtons.Middle;
            if (newMouse.XButton1 == ButtonState.Pressed) buttons |= MouseButtons.XButton1;
            if (newMouse.XButton2 == ButtonState.Pressed) buttons |= MouseButtons.XButton2;

            return buttons;
        }

        private static MouseButtons GetReleasedButtons()
        {
            return (newCurrentButtons ^ previousCurrentButtons) & previousCurrentButtons;
        }

        public static bool JustPressed(Keys key)
        {
            return newKeyboard.IsKeyDown(key) && !previousKeyboard.IsKeyDown(key);
        }

        public static bool CurrentlyPressed(Keys key)
        {
            return newKeyboard.IsKeyDown(key) && previousKeyboard.IsKeyDown(key);
        }

        public static bool PressLeaved(Keys key)
        {
            return !newKeyboard.IsKeyDown(key) && previousKeyboard.IsKeyDown(key);
        }

        public static bool AllPressed(params Keys[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (!newKeyboard.IsKeyDown(keys[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool AnyJustPressed(MouseButtons button)
        {
            if (button.HasFlag(MouseButtons.Left) && newMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton != ButtonState.Pressed)
                return true;
            else if (button.HasFlag(MouseButtons.Right) && newMouse.RightButton == ButtonState.Pressed && previousMouse.RightButton != ButtonState.Pressed)
                return true;
            else if (button.HasFlag(MouseButtons.Middle) && newMouse.MiddleButton == ButtonState.Pressed && previousMouse.MiddleButton != ButtonState.Pressed)
                return true;
            else if (button.HasFlag(MouseButtons.XButton1) && newMouse.XButton1 == ButtonState.Pressed && previousMouse.XButton1 != ButtonState.Pressed)
                return true;
            else if (button.HasFlag(MouseButtons.XButton2) && newMouse.XButton2 == ButtonState.Pressed && previousMouse.XButton2 != ButtonState.Pressed)
                return true;

            return false;
        }

        public static bool AllJustPressed(MouseButtons button)
        {
            if (button.HasFlag(MouseButtons.Left) && !(newMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton != ButtonState.Pressed))
                return false;
            else if (button.HasFlag(MouseButtons.Right) && !(newMouse.RightButton == ButtonState.Pressed && previousMouse.RightButton != ButtonState.Pressed))
                return false;
            else if (button.HasFlag(MouseButtons.Middle) && !(newMouse.MiddleButton == ButtonState.Pressed && previousMouse.MiddleButton != ButtonState.Pressed))
                return false;
            else if (button.HasFlag(MouseButtons.XButton1) && !(newMouse.XButton1 == ButtonState.Pressed && previousMouse.XButton1 != ButtonState.Pressed))
                return false;
            else if (button.HasFlag(MouseButtons.XButton2) && !(newMouse.XButton2 == ButtonState.Pressed && previousMouse.XButton2 != ButtonState.Pressed))
                return false;

            return true;
        }

        public static bool AnyCurrentlyPressed(MouseButtons button)
        {
            if (button.HasFlag(MouseButtons.Left) && newMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Pressed)
                return true;
            else if (button.HasFlag(MouseButtons.Right) && newMouse.RightButton == ButtonState.Pressed && previousMouse.RightButton == ButtonState.Pressed)
                return true;
            else if (button.HasFlag(MouseButtons.Middle) && newMouse.MiddleButton == ButtonState.Pressed && previousMouse.MiddleButton == ButtonState.Pressed)
                return true;
            else if (button.HasFlag(MouseButtons.XButton1) && newMouse.XButton1 == ButtonState.Pressed && previousMouse.XButton1 == ButtonState.Pressed)
                return true;
            else if (button.HasFlag(MouseButtons.XButton2) && newMouse.XButton2 == ButtonState.Pressed && previousMouse.XButton2 == ButtonState.Pressed)
                return true;

            return false;
        }

        public static bool AllCurrentlyPressed(MouseButtons button)
        {
            if (button.HasFlag(MouseButtons.Left) && !(newMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Pressed))
                return false;
            else if (button.HasFlag(MouseButtons.Right) && !(newMouse.RightButton == ButtonState.Pressed && previousMouse.RightButton == ButtonState.Pressed))
                return false;
            else if (button.HasFlag(MouseButtons.Middle) && !(newMouse.MiddleButton == ButtonState.Pressed && previousMouse.MiddleButton == ButtonState.Pressed))
                return false;
            else if (button.HasFlag(MouseButtons.XButton1) && !(newMouse.XButton1 == ButtonState.Pressed && previousMouse.XButton1 == ButtonState.Pressed))
                return false;
            else if (button.HasFlag(MouseButtons.XButton2) && !(newMouse.XButton2 == ButtonState.Pressed && previousMouse.XButton2 == ButtonState.Pressed))
                return false;

            return true;
        }

        public static bool AnyPressLeaved(MouseButtons button)
        {
            if (button.HasFlag(MouseButtons.Left) && newMouse.LeftButton != ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Pressed)
                return true;
            else if (button.HasFlag(MouseButtons.Right) && newMouse.RightButton != ButtonState.Pressed && previousMouse.RightButton == ButtonState.Pressed)
                return true;
            else if (button.HasFlag(MouseButtons.Middle) && newMouse.MiddleButton != ButtonState.Pressed && previousMouse.MiddleButton == ButtonState.Pressed)
                return true;
            else if (button.HasFlag(MouseButtons.XButton1) && newMouse.XButton1 != ButtonState.Pressed && previousMouse.XButton1 == ButtonState.Pressed)
                return true;
            else if (button.HasFlag(MouseButtons.XButton2) && newMouse.XButton2 != ButtonState.Pressed && previousMouse.XButton2 == ButtonState.Pressed)
                return true;

            return false;
        }

        public static bool AllPressLeaved(MouseButtons button)
        {
            if (button.HasFlag(MouseButtons.Left) && !(newMouse.LeftButton != ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Pressed))
                return false;
            else if (button.HasFlag(MouseButtons.Right) && !(newMouse.RightButton != ButtonState.Pressed && previousMouse.RightButton == ButtonState.Pressed))
                return false;
            else if (button.HasFlag(MouseButtons.Middle) && !(newMouse.MiddleButton != ButtonState.Pressed && previousMouse.MiddleButton == ButtonState.Pressed))
                return false;
            else if (button.HasFlag(MouseButtons.XButton1) && !(newMouse.XButton1 != ButtonState.Pressed && previousMouse.XButton1 == ButtonState.Pressed))
                return false;
            else if (button.HasFlag(MouseButtons.XButton2) && !(newMouse.XButton2 != ButtonState.Pressed && previousMouse.XButton2 == ButtonState.Pressed))
                return false;

            return true;
        }
    }
}
