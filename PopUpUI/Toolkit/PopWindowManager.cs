using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Input;
using PopUpUI.Surface;
using System.Collections.Generic;

namespace PopUpUI.Toolkit
{
    public static class PopWindowManager
    {
        private static bool isInitialized = false;

        /// <summary>Gets or sets the scale to use when drawing all the windows to screen. Default value is 1.</summary>
        public static float GlobalScale { get; set; } = 1;
        /// <summary>Gets or sets the opacity to use when drawing all the windows to screen. Default value is 1.</summary>
        public static float GlobalOpacity { get; set; } = 1;

        public static List<PopWindow> ActiveWindows { get; } = new List<PopWindow>();

        internal static void Initialize()
        {
            if (isInitialized) return;

            InputManager.KeyDown += InputManager_KeyDown;
            InputManager.KeyUp += InputManager_KeyUp;
            InputManager.MouseButtonStateChange += InputManager_MouseButtonStateChange;
            InputManager.MousePositionChange += InputManager_MousePositionChange;
            InputManager.MouseWheelChange += InputManager_MouseWheelChange;
            InputManager.TextInput += InputManager_TextInput;
            InputManager.FileDrop += InputManager_FileDrop;

            isInitialized = true;
        }

        internal static void FrameUpdate()
        {
            foreach (var window in ActiveWindows) window.Update();
        }

        internal static void RenderUpdate()
        {
            foreach (var window in ActiveWindows) window.RenderUpdate();
        }

        internal static void BufferDraw(SpriteBatch batch)
        {
            foreach (var window in ActiveWindows)
            {
                batch.Draw(window.WindowTexture, 
                           new Rectangle(window.Boundaries.Location.TimesFloat(GlobalScale), window.Boundaries.Size.TimesFloat(window.Scale * GlobalScale)), 
                           Color.White * GlobalOpacity);
            }
        }

        private static PopUpWindowEventArgs ScaledWindowArgs(PopWindow window, PopUpWindowEventArgs e)
        {
            return e with { MousePoint = (e.MousePoint.TimesFloat(1 / GlobalScale) - window.Boundaries.Location).TimesFloat(1 / window.Scale) };
        }

        private static void InputManager_KeyDown(PopUpWindowEventArgs e, Microsoft.Xna.Framework.Input.Keys arg)
        {
            foreach (var window in ActiveWindows)
            {
                window.OnKeyDown(ScaledWindowArgs(window, e), arg);
            }
        }

        private static void InputManager_KeyUp(PopUpWindowEventArgs e, Microsoft.Xna.Framework.Input.Keys arg)
        {
            foreach (var window in ActiveWindows)
            {
                window.OnKeyUp(ScaledWindowArgs(window, e), arg);
            }
        }

        private static void InputManager_MouseButtonStateChange(PopUpWindowEventArgs e)
        {
            foreach (var window in ActiveWindows)
            {
                window.OnMouseButtonStateChange(ScaledWindowArgs(window, e));
            }
        }

        private static void InputManager_MousePositionChange(PopUpWindowEventArgs e)
        {
            foreach (var window in ActiveWindows)
            {
                window.OnMousePositionChange(ScaledWindowArgs(window, e));
            }
        }

        private static void InputManager_MouseWheelChange(PopUpWindowEventArgs e)
        {
            foreach (var window in ActiveWindows)
            {
                window.OnMouseWheelChange(ScaledWindowArgs(window, e));
            }
        }

        private static void InputManager_TextInput(PopUpWindowEventArgs e, char arg)
        {
            foreach (var window in ActiveWindows)
            {
                window.OnTextInput(ScaledWindowArgs(window, e), arg);
            }
        }

        private static void InputManager_FileDrop(PopUpWindowEventArgs e, FileDropEventArgs arg)
        {
            foreach (var window in ActiveWindows)
            {
                window.OnFileDrop(ScaledWindowArgs(window, e), arg);
            }
        }
    }
}
