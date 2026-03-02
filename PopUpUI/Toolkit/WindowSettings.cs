using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PopUpUI.Elements;
using PopUpUI.Surface;
using PopUpUI.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using PopUpUI.Surface.Animation;

namespace PopUpUI.Toolkit
{
    /// <summary>Contains properties and methods to help changing features of the main window.</summary>
    public static class WindowSettings
    {
        private static bool isInitialized = false;

        private static Game gameInstance;
        private static GraphicsDeviceManager managerInstance;
        private static CustomCursor cursor;

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
            set
            {
                if (value == true) RemoveCustomCursor();

                gameInstance.IsMouseVisible = value;
            }
        }
        /// <summary>Indicates if users can resize main window. Default value is <see langword="true"/>.</summary>
        public static bool AllowUserResizing
        {
            get => gameInstance.Window.AllowUserResizing;
            set
            {
                if (value == true)
                {
                    ExtenderManager.RemoveExtenders();
                }

                gameInstance.Window.AllowUserResizing = value;
            }
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

            // This line stops the window from centering itself when its size changes
            // if this line gets removed, in right and bottom extenders position of the window must be stored before size change and set to stored value after size change.
            game.Window.Position = game.Window.Position;

            cursor = new CustomCursor();

            isInitialized = true;
        }

        #region internals
        internal static void InternalExtenderUpdate(PopUpWindowEventArgs e)
        {
            ExtenderManager.MousePointChangeUpdate(e);
        }

        internal static void InternalDraw(SpriteBatch batch)
        {
            if (cursor.InUse)
            {
                batch.Draw(cursor.Animation, (InputManager.MousePosition - cursor.Origin).ToVector2(), Color.White);
            }
        }
        #endregion

        #region Cursor
        /// <summary>Sets the mouse cursor to <paramref name="cursor"/>.</summary>
        /// <param name="cursor">The new mouse cursor.</param>
        public static void SetCursor(MouseCursor cursor)
        {
            RemoveCustomCursor();

            IsMouseVisible = true;
            Mouse.SetCursor(cursor ?? MouseCursor.Arrow);
        }

        /// <summary>Sets a mouse cursor which supports animation and drawn on screen like a sprite.</summary>
        /// <param name="animation">The visual of the cursor.</param>
        /// <param name="xOrigin">X coordinate of the image that will be used for mouse position.</param>
        /// <param name="yOrigin">Y coordinate of the image that will be used for mouse position.</param>
        public static void SetCursor(AnimationState animation, int xOrigin, int yOrigin)
        {
            IsMouseVisible = false;

            cursor.Animation = animation;
            cursor.Origin = new Point(xOrigin, yOrigin);
            cursor.Position = InputManager.MousePosition - cursor.Origin;
            cursor.InUse = true;
        }

        private static void RemoveCustomCursor()
        {
            cursor.InUse = false;
        }
        #endregion

        #region Window
        /// <summary>Makes the main window borderless, which removes titlebar and ability to resize the main window.</summary>
        /// <remarks>To be able to resize main window, <paramref name="useExtenders"/> can be set to <see langword="true"/>, which creates 
        /// invisible extenders on the edges of the window in a small width, which helps to resize the window.</remarks>
        /// <param name="useExtenders">A preference to create extenders on the edges of the window, to be able to resize the window. 
        /// If set to <see langword="false"/>, window cannot be resized with borders of the window.</param>
        public static void SetToBorderless(bool useExtenders)
        {
            gameInstance.Window.IsBorderless = true;

            if (useExtenders)
            {
                ExtenderManager.AddExtenders();
            }
        }

        /// <summary>Brings back the titlebar and the ability of being able to resize the main window.</summary>
        public static void SetToBordered()
        {
            gameInstance.Window.IsBorderless = false;

            ExtenderManager.RemoveExtenders();
        }

        /// <summary>Changes window's position to appear center of the screen.</summary>
        public static void CenterTheWindow()
        {
            WindowPosition = new Point((GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - Width) / 2,
                                       (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - Height) / 2);
        }
        #endregion

        private class CustomCursor
        {
            public bool InUse { get; set; }
            public Point Origin { get; set; }
            public Point Position { get; set; }
            public AnimationState Animation { get; set; }
        }

        private static class ExtenderManager
        {
            private static bool isAdded = false;
            private static IEnumerable<Extender> Extenders { get; } = [new Extender(Extender.Directions.Top), new Extender(Extender.Directions.Bottom),
                                                                      new Extender(Extender.Directions.Left), new Extender(Extender.Directions.Right)];

            public static void AddExtenders() => isAdded = true;
            public static void RemoveExtenders() => isAdded = false;

            public static void MousePointChangeUpdate(PopUpWindowEventArgs e)
            {
                if (!isAdded) return;

                foreach (var element in Extenders)
                {
                    if (element.CanBeHovered(e.MousePoint))
                    {
                        if (!element.IsHovered) element.OnHoverStateChange(e, true);
                    }
                    else if (element.IsHovered)
                    {
                        element.OnHoverStateChange(e, false);
                    }
                }

                IEnumerable<Extender> holdableElements = Extenders.Where(ex => ex.CanBeHeld(e.MousePoint) || ex.IsHeld);
                IEnumerable<Extender> alreadyHeldElements = holdableElements.ToList().Where(element => element.IsHeld);

                foreach (var holdable in alreadyHeldElements)
                {
                    holdable.HoldStatusUpdate(e, holdable.ButtonsToHold.HasAnyFlagFrom(e.PressedButtons));
                }

                foreach (var element in holdableElements.Except(alreadyHeldElements))
                {
                    if (element.CanBeHeld(e.MousePoint))
                    {
                        element.HoldStatusUpdate(e, element.ButtonsToHold.HasAnyFlagFrom(e.PressedButtons));
                    }
                }
            }
        }

        private class Extender
        {
            public Point StartingSize;

            private bool isHeld;
            private bool isHovered;

            public bool IsHeld => isHeld;
            public bool IsHovered => isHovered;
            public Directions Direction { get; }
            public MouseButtons ButtonsToHold { get; }

            public enum Directions
            {
                Top,
                Bottom,
                Left,
                Right
            }

            public Extender(Directions dir)
            {
                Direction = dir;
                ButtonsToHold = MouseButtons.Left;
            }

            public bool CanBeHeld(Point point)
            {
                return Direction switch
                {
                    Directions.Top => 0 <= point.Y && point.Y <= 5,
                    Directions.Bottom => Height - 5 <= point.Y && point.Y <= Height,
                    Directions.Left => 0 <= point.X && point.X <= 5,
                    Directions.Right => Width - 5 <= point.X && point.X <= Width,
                    _ => throw new NotImplementedException()
                };
            }

            public bool CanBeHovered(Point point)
            {
                return CanBeHeld(point);
            }

            public void HoldStatusUpdate(PopUpWindowEventArgs e, bool isHeld)
            {
                if (this.isHeld && !isHeld)
                {
                    HoldStateChangeEvent(e, false);
                }
                else if (!this.isHeld && isHeld)
                {
                    HoldStateChangeEvent(e, true);
                }

                this.isHeld = isHeld;

                if (isHeld)
                {
                    HeldEvent(e);
                }
            }

            public void OnHoverStateChange(PopUpWindowEventArgs e, bool isHovered)
            {
                this.isHovered = isHovered;
                HoverStateChangeEvent(e, isHovered);
            }

            private void HoldStateChangeEvent(PopUpWindowEventArgs e, bool isHeld)
            {
                if (isHeld)
                {
                    StartingSize = e.MousePoint;
                }
            }

            private void HoverStateChangeEvent(PopUpWindowEventArgs e, bool isHovered)
            {
                Mouse.SetCursor(isHovered ? (Direction == Directions.Top || Direction == Directions.Bottom ? MouseCursor.SizeNS
                                                                                                           : MouseCursor.SizeWE)
                                          : MouseCursor.Arrow);
            }

            private void HeldEvent(PopUpWindowEventArgs e)
            {
                Point change = e.MousePoint - StartingSize;

                switch (Direction)
                {
                    case Directions.Top:
                        WindowSize = new Point(WindowSize.X, WindowSize.Y - change.Y);
                        WindowPosition = new Point(WindowPosition.X, WindowPosition.Y + change.Y);

                        break;
                    case Directions.Left:
                        WindowSize = new Point(WindowSize.X - change.X, WindowSize.Y);
                        WindowPosition = new Point(WindowPosition.X + change.X, WindowPosition.Y);

                        break;
                    case Directions.Bottom:
                        WindowSize = new Point(WindowSize.X, WindowSize.Y + change.Y);
                        StartingSize = e.MousePoint;

                        break;
                    case Directions.Right:
                        WindowSize = new Point(WindowSize.X + change.X, WindowSize.Y);
                        StartingSize = e.MousePoint;

                        break;
                }
            }
        }
    }
}
