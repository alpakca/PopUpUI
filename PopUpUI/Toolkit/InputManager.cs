using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PopUpUI.Elements;
using PopUpUI.Input;
using System.Diagnostics;

namespace PopUpUI.Toolkit
{
    /// <summary>Handler for the <see cref="InputManager"/> events.</summary>
    /// <param name="e">A <see cref="PopUpWindowEventArgs"/> object that contains event data.</param>
    public delegate void InputManagerHandler(PopUpWindowEventArgs e);
    /// <summary>Handler for the <see cref="InputManager"/> events with an extra argument.</summary>
    /// <param name="e">A <see cref="PopUpWindowEventArgs"/> object that contains event data.</param>
    /// <param name="arg">An extra argument for the handler beside <see cref="PopUpWindowEventArgs"/>.</param>
    public delegate void InputManagerHandler<TExtraArgs>(PopUpWindowEventArgs e, TExtraArgs arg);

    /// <summary>Represents a manager for input events.</summary>
    public static class InputManager
    {
        private static KeyboardState oldKeyboardState;
        private static MouseState oldMouseState;
        private static KeyboardState newKeyboardState = Keyboard.GetState();
        private static MouseState newMouseState = Mouse.GetState();
        private static float pointChangeCooldown;

        /// <summary>Raised when any button of the mouse have been pressed.</summary>
        public static event InputManagerHandler MouseButtonStateChange;
        /// <summary>Raised when the position of the mouse has changed.</summary>
        public static event InputManagerHandler MousePositionChange;
        /// <summary>Raised when the mouse's wheel's value has changed.</summary>
        public static event InputManagerHandler MouseWheelChange;
        /// <summary>Raised when a key is down on keyboard.</summary>
        public static event InputManagerHandler<Keys> KeyDown;
        /// <summary>Raised when a key is up on keyboard.</summary>
        public static event InputManagerHandler<Keys> KeyUp;
        /// <summary>Raised when there is a text entry has been made.</summary>
        public static event InputManagerHandler<char> TextInput;
        /// <summary>Raised when a file has been dropped on main window.</summary>
        public static event InputManagerHandler<FileDropEventArgs> FileDrop;

        /// <summary>Indicates if the Num Lock is on.</summary>
        public static bool NumLock => newKeyboardState.NumLock;
        /// <summary>Indicates if the Caps Lock is on.</summary>
        public static bool CapsLock => newKeyboardState.CapsLock;
        /// <summary>Gets the position of the mouse.</summary>
        public static Point MousePosition => newMouseState.Position;

        /// <summary>Gets or sets the max time <see cref="MousePositionChange"/> event 
        /// will be raised in case <see cref="MousePosition"/> didn't change, in milliseconds. Default is 32.</summary>
        public static float MaxCooldownPointChange { get; set; } = 32;

        internal static void Initialize(Game game)
        {         
            game.Window.TextInput += (_, e) =>
            {
                TextInput(new PopUpWindowEventArgs(GetPressedButtons(ref newMouseState),
                                                   GetNewlyPressedButtons(),
                                                   MousePosition,
                                                   newMouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue),
                          e.Character);
            };

            game.Window.KeyDown += (_, e) =>
            {
                KeyDown(new PopUpWindowEventArgs(GetPressedButtons(ref newMouseState),
                                                 GetNewlyPressedButtons(),
                                                 MousePosition,
                                                 newMouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue), 
                        e.Key);
            };

            game.Window.KeyUp += (_, e) =>
            {
                KeyUp(new PopUpWindowEventArgs(GetPressedButtons(ref newMouseState),
                                               GetNewlyPressedButtons(),
                                               MousePosition,
                                               newMouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue),
                      e.Key);
            };

            game.Window.FileDrop += (_, e) =>
            {
                FileDrop(new PopUpWindowEventArgs(GetPressedButtons(ref newMouseState),
                                                  GetNewlyPressedButtons(),
                                                  MousePosition,
                                                  newMouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue),
                         e);  
            };
        }
        
        internal static void FrameUpdate()
        {
            oldKeyboardState = newKeyboardState;
            oldMouseState = newMouseState;

            newKeyboardState = Keyboard.GetState();
            newMouseState = Mouse.GetState();

            PopUpWindowEventArgs args = new PopUpWindowEventArgs(GetPressedButtons(ref newMouseState),
                                                                 GetNewlyPressedButtons(),
                                                                 newMouseState.Position,
                                                                 newMouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue);
            
            if (args.NewlyPressedButtons != MouseButtons.None)
            {
                MouseButtonStateChange(args);
            }

            pointChangeCooldown -= Time.DeltaTime;
            if (pointChangeCooldown < 0 || oldMouseState.Position != newMouseState.Position)
            {
                pointChangeCooldown = MaxCooldownPointChange;
                MousePositionChange(args);
            }

            if (oldMouseState.ScrollWheelValue != newMouseState.ScrollWheelValue)
            {
                MouseWheelChange(args);
            }
        }

        /// <summary>Checks if <paramref name="key"/> is down in newest frame.</summary>
        /// <param name="key">The key to check.</param>
        /// <returns><see langword="true"/> if <paramref name="key"/> is down in newest frame; otherwise <see langword="false"/>.</returns>
        public static bool IsDown(Keys key)
        {
            return newKeyboardState.IsKeyDown(key);
        }

        /// <summary>Checks if <paramref name="key"/> is up in newest frame.</summary>
        /// <param name="key">The key to check.</param>
        /// <returns><see langword="true"/> if <paramref name="key"/> is up in newest frame; otherwise <see langword="false"/>.</returns>
        public static bool IsUp(Keys key)
        {
            return !IsDown(key);
        }

        /// <summary>Checks if <paramref name="key"/> is being held.</summary>
        /// <param name="key">The key to check.</param>
        /// <returns><see langword="true"/> if <paramref name="key"/> is being held; otherwise <see langword="false"/>.</returns>
        public static bool IsHeld(Keys key)
        {
            return newKeyboardState.IsKeyDown(key) && oldKeyboardState.IsKeyDown(key);
        }

        /// <summary>Checks if <paramref name="key"/> is just pressed with newest frame.</summary>
        /// <param name="key">The key to check.</param>
        /// <returns><see langword="true"/> if <paramref name="key"/> is just pressed with newest frame; otherwise <see langword="false"/>.</returns>
        public static bool IsJustPressed(Keys key)
        {
            return newKeyboardState.IsKeyDown(key) && !oldKeyboardState.IsKeyDown(key);
        }

        /// <summary>Checks if <paramref name="key"/> is released with newest frame.</summary>
        /// <param name="key">The key to check.</param>
        /// <returns><see langword="true"/> if <paramref name="key"/> is released with newest frame; otherwise <see langword="false"/>.</returns>
        public static bool IsReleased(Keys key)
        {
            return !newKeyboardState.IsKeyDown(key) && oldKeyboardState.IsKeyDown(key);
        }

        /// <summary>Checks whether given key combination can be used through down keys.</summary>
        /// <param name="keys">The combination to check.</param>
        /// <returns><see langword="true"/> if combination can be used; otherwise <see langword="false"/>.</returns>
        public static bool CanUseCombination(params Keys[] keys)
        {
            foreach (var key in keys) if (!IsDown(key)) return false;

            return true;
        }

        /// <summary>Checks if <paramref name="button"/> is down in newest frame.</summary>
        /// <param name="button">The mouse button to check.</param>
        /// <returns><see langword="true"/> if <paramref name="button"/> is down in newest frame; otherwise <see langword="false"/>.</returns>
        public static bool IsDown(MouseButtons button)
        {
            return GetPressedButtons(ref newMouseState).HasFlag(button);
        }

        /// <summary>Checks if <paramref name="button"/> is up in newest frame.</summary>
        /// <param name="button">The mouse button to check.</param>
        /// <returns><see langword="true"/> if <paramref name="button"/> is up in newest frame; otherwise <see langword="false"/>.</returns>
        public static bool IsUp(MouseButtons button)
        {
            return !IsDown(button);
        }

        /// <summary>Checks if <paramref name="button"/> is being held.</summary>
        /// <param name="button">The mouse button to check.</param>
        /// <returns><see langword="true"/> if <paramref name="button"/> is being held; otherwise <see langword="false"/>.</returns>
        public static bool IsHeld(MouseButtons button)
        {
            return GetPressedButtons(ref newMouseState).HasFlag(button) && GetPressedButtons(ref oldMouseState).HasFlag(button);
        }

        /// <summary>Checks if <paramref name="button"/> is just pressed with newest frame.</summary>
        /// <param name="button">The mouse button to check.</param>
        /// <returns><see langword="true"/> if <paramref name="button"/> is just pressed with newest frame; otherwise <see langword="false"/>.</returns>
        public static bool IsJustPressed(MouseButtons button)
        {
            return GetPressedButtons(ref newMouseState).HasFlag(button) && !GetPressedButtons(ref oldMouseState).HasFlag(button);
        }

        /// <summary>Checks if <paramref name="button"/> is released with newest frame.</summary>
        /// <param name="button">The mouse button to check.</param>
        /// <returns><see langword="true"/> if <paramref name="button"/> is released with newest frame; otherwise <see langword="false"/>.</returns>
        public static bool IsReleased(MouseButtons button)
        {
            return !GetPressedButtons(ref newMouseState).HasFlag(button) && GetPressedButtons(ref oldMouseState).HasFlag(button);
        }

        private static MouseButtons GetNewlyPressedButtons()
        {
            MouseButtons newlyPressedButtons = MouseButtons.None;

            if (IsJustPressed(MouseButtons.Left)) newlyPressedButtons |= MouseButtons.Left;
            if (IsJustPressed(MouseButtons.Right)) newlyPressedButtons |= MouseButtons.Right;
            if (IsJustPressed(MouseButtons.Middle)) newlyPressedButtons |= MouseButtons.Middle;
            if (IsJustPressed(MouseButtons.XButton1)) newlyPressedButtons |= MouseButtons.XButton1;
            if (IsJustPressed(MouseButtons.XButton2)) newlyPressedButtons |= MouseButtons.XButton2;

            return newlyPressedButtons ^ MouseButtons.None;
        }

        private static MouseButtons GetPressedButtons(ref readonly MouseState state)
        {
            MouseButtons pressedButtons = MouseButtons.None;

            if (state.LeftButton == ButtonState.Pressed) pressedButtons |= MouseButtons.Left;
            if (state.RightButton == ButtonState.Pressed) pressedButtons |= MouseButtons.Right;
            if (state.MiddleButton == ButtonState.Pressed) pressedButtons |= MouseButtons.Middle;
            if (state.XButton1 == ButtonState.Pressed) pressedButtons |= MouseButtons.XButton1;
            if (state.XButton2 == ButtonState.Pressed) pressedButtons |= MouseButtons.XButton2;

            return pressedButtons ^ MouseButtons.None;
        }
    }
}
