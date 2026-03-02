using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PopUpUI.Toolkit
{
    /// <summary>Contains properties and methods to help changing features of the main window.</summary>
    public static class WindowOptions
    {
        private static bool isInitialized = false;

        private static Game gameInstance;
        private static GraphicsDeviceManager managerInstance;

        /// <summary>Gets the width of the main window.</summary>
        public static int Width => managerInstance.PreferredBackBufferWidth;
        /// <summary>Gets the height of the main window.</summary>
        public static int Height => managerInstance.PreferredBackBufferHeight;
        /// <summary>Indicates if the window is borderless. To change, use <see cref="SetToBordered"/> or <see cref="SetToBorderless(bool)"/>.</summary>
        public static bool IsBorderless => gameInstance.Window.IsBorderless;

        /// <summary>Indicates the desire to switch into fullscreen mode.</summary>
        public static bool IsFullScreen
        {
            get => managerInstance.IsFullScreen;
            set
            {
                managerInstance.IsFullScreen = value;
                managerInstance.ApplyChanges();
            }
        }

        /// <summary>Gets or sets the visibility of the cursor.</summary>
        public static bool IsMouseVisible
        {
            get => gameInstance.IsMouseVisible;
            set => gameInstance.IsMouseVisible = value;
        }

        /// <summary>Indicates if users can resize main window. Default value is <see langword="true"/>.</summary>
        public static bool AllowUserResizing
        {
            get => gameInstance.Window.AllowUserResizing;
            set => gameInstance.Window.AllowUserResizing = value;
        }

        /// <summary>Gets or sets a bool that enables usage of Alt+F4 for main window closing on desktop platforms. Default value is <see langword="true"/>.</summary>
        public static bool AllowAltF4
        {
            get => gameInstance.Window.AllowAltF4;
            set => gameInstance.Window.AllowAltF4 = value;
        }

        /// <summary>Indicates the desire for vsync when presenting the back buffer.</summary>
        public static bool UseVsync
        {
            get => managerInstance.SynchronizeWithVerticalRetrace;
            set
            {
                managerInstance.SynchronizeWithVerticalRetrace = value;
                managerInstance.ApplyChanges(); 
            }
        }

        /// <summary>Gets or sets the position of the main window.</summary>
        public static Point WindowPosition
        {
            get => gameInstance.Window.Position;
            set => gameInstance.Window.Position = value;
        }

        /// <summary>Gets or sets the size of the main window.</summary>
        public static Point WindowSize
        {
            get => new Point(Width, Height);
            set
            {
                managerInstance.PreferredBackBufferWidth = value.X;
                managerInstance.PreferredBackBufferHeight = value.Y;
                managerInstance.ApplyChanges();
            }
        }

        internal static void Initialize(Game game, GraphicsDeviceManager manager)
        {
            if (isInitialized) return;
            
            gameInstance = game;
            managerInstance = manager;

            isInitialized = true;
        }

        /// <summary>Sets the mouse cursor to <paramref name="cursor"/>.</summary>
        /// <param name="cursor">The new mouse cursor.</param>
        public static void SetCursor(MouseCursor cursor)
        {
            IsMouseVisible = true;
            Mouse.SetCursor(cursor ?? MouseCursor.Arrow);
        }

        /// <summary>Makes the main window borderless, which removes titlebar and ability to resize the main window.</summary>
        /// <remarks>To be able to resize main window, <paramref name="useExtenders"/> can be set to <see langword="true"/>, which creates 
        /// invisible extenders on the edges of the window in a small width, which helps to resize the window.</remarks>
        /// <param name="useExtenders">A preference to create extenders on the edges of the window, to be able to resize the window. 
        /// If set to <see langword="false"/>, window cannot be resized with borders of the window.</param>
        public static void Borderless()
        {
            gameInstance.Window.IsBorderless = true;
        }

        /// <summary>Brings back the titlebar and the ability of being able to resize the main window.</summary>
        public static void Bordered()
        {
            gameInstance.Window.IsBorderless = false;
        }

        /// <summary>Changes window's position to appear center of the screen.</summary>
        public static void CenterWindow()
        {
            WindowPosition = new Point((GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - Width) / 2,
                                       (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - Height) / 2);
        }
    }
}
