using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PopUpUI.Containers;
using PopUpUI.Elements;
using PopUpUI.Input;
using PopUpUI.Surface.Animation;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PopUpUI.Surface
{
    /// <summary>Represents a window that can contain <see cref="PopUpElement"/>s and <see cref="PopUpWindow"/>s. It is the main object of PopUpUI.</summary>
    public sealed class PopUpWindow
    {
        private static readonly Action<PopUpWindow> WindowEmptyDelegate = delegate { };
        private static readonly PopUpWindowHandler WindowEventEmptyDelegate = delegate { };

        private static bool IsInitialized = false;
        private static readonly Stack<PopUpWindow> WindowLayers = new Stack<PopUpWindow>();

        /// <summary>Gets or sets the scale to use when drawing all the windows to screen. Default value is 1.</summary>
        public static float GlobalScale { get; set; } = 1;
        /// <summary>Gets or sets the opacity to use when drawing all the windows to screen. Default value is 1.</summary>
        public static float GlobalOpacity { get; set; } = 1;

        private bool isFrozen;
        private Rectangle bounds;
        private RenderTarget2D windowTexture;
        private readonly TextureDrawOptions drawOptions;

        /// <summary>Indicates if window is currently frozen.</summary>
        public bool IsFrozen => isFrozen;
        /// <summary>Gets the texture of the window.</summary>
        public RenderTarget2D WindowTexture => windowTexture;
        /// <summary><see cref="PopUpElement"/> elements of window.</summary>
        public PopUpElementCollection Elements { get; }
        /// <summary>Children of this window, windows that can receive the same event data as this window and are drawn with this window.</summary>
        public List<PopUpWindow> Children { get; }
        /// <summary>Presents the keys and their actions that will be triggered when their key is pressed.</summary>
        public Dictionary<Keys, PopUpWindowHandler> Shortcuts { get; }

        /// <summary>Enables more than one <see cref="IHoldable"/> instances to be updated at the same time. Default value is <see langword="true"/>.</summary>
        public bool EnableMultiHold { get; set; } = true;
        /// <summary>Enables more than one <see cref="IHoverable"/> instances to be updated at the same time. Default value is <see langword="true"/>.</summary>
        public bool EnableMultiHover { get; set; } = true;
        /// <summary>Lets user to close the window by left or right clicking outside bounds of this window. Default value is <see langword="false"/>.</summary>
        public bool CloseWithSelectingOutside { get; set; }
        /// <summary>Blocks <see cref="IKeyInputReceiver"/> events if any shortcut's key was triggered. Default value is <see langword="false"/>.</summary>
        public bool BlockKeyInputEventsWithShortcuts { get; set; }
        /// <summary>Gets or sets a preference if the window will be automatically frozen or unfreezed if it losts or regains its peek status.</summary>
        public bool FreezeUponPeekIsLost { get; set; }
        /// <summary>The opacity of window. Expected inputs are between 0 and 1. Zero makes it invisible and one makes images' look original.</summary>
        public float Opacity { get; set; } = 1;
        /// <summary>Gets or sets the scale to use when drawing this window to screen. It doesn't effect bounds or texture of <see cref="WindowTexture"/>.</summary>
        public float Scale { get; set; } = 1;
        /// <summary>Gets or sets the options to be used for freezing the window and its elements. Default value is <see cref="FreezingOptions.Nothing"/>.</summary>
        /// <remarks>Elements' own options doesn't change with this pass, it is one time only.</remarks>
        public FreezingOptions FreezingOption { get; set; } = FreezingOptions.Nothing;
        /// <summary>Bounds of window, that is position and size of window. The size doesn't change the original size of elements, only effects the shown area.</summary>
        public Rectangle Bounds
        {
            get => bounds;
            set
            {
                if (value.Width <= 0 || value.Height <= 0) throw new ArgumentOutOfRangeException();

                if (bounds.Size != value.Size)
                {
                    windowTexture.Dispose();
                    windowTexture = DrawHelper.CreateRenderTarget(value.Width, value.Height);
                }

                bounds = value;
            }
        }
        /// <summary>Gets or sets a background animation for window.</summary>
        public AnimationState BackgroundAnimation { get; set; }

        /// <summary>Raised when window has added.</summary>
        public event Action<PopUpWindow> Added = WindowEmptyDelegate;
        /// <summary>Raised when window has removed.</summary>
        public event Action<PopUpWindow> Removed = WindowEmptyDelegate;
        /// <summary>Raised when window has frozen.</summary>
        public event Action<PopUpWindow> Frozen = WindowEmptyDelegate;
        /// <summary>Raised when window has unfreezed.</summary>
        public event Action<PopUpWindow> Unfreezed = WindowEmptyDelegate;
        /// <summary>Raised when left or right mouse button has changed its state.</summary>
        public event PopUpWindowHandler MouseButtonChange = WindowEventEmptyDelegate;
        /// <summary>Raised when mouse position has changed.</summary>
        public event PopUpWindowHandler MousePointChange = WindowEventEmptyDelegate;
        /// <summary>Raised when mouse wheel value has changed.</summary>
        public event PopUpWindowHandler MouseWheelChange = WindowEventEmptyDelegate;
        /// <summary>Raised when a key is down on keyboard.</summary>
        public event PopUpWindowHandler<Keys> KeyDown = delegate { };
        /// <summary>Raised when a key's down status is over on keyboard.</summary>
        public event PopUpWindowHandler<Keys> KeyUp = delegate { };
        /// <summary>Raised when a text input has been made on keyboard.</summary>
        public event PopUpWindowHandler<char> TextInput = delegate { };
        /// <summary>Raised when a file has been dropped on the window.</summary>
        public event PopUpWindowHandler<FileDropEventArgs> FileDrop = delegate { };

#nullable enable
        /// <summary>Creates a new window which has a size of given <paramref name="width"/> and <paramref name="height"/> and uses <paramref name="options"/> while drawing.</summary>
        /// <param name="width">Width of window.</param>
        /// <param name="height">Height of window.</param>
        /// <param name="options">Options to control drawing behaviour.</param>
        public PopUpWindow(int width, int height, TextureDrawOptions? options)
        {
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException("Width and height must be greater than zero.");
            bounds = new Rectangle(0, 0, width, height);

            windowTexture = DrawHelper.CreateRenderTarget(width, height);
            drawOptions = options ?? TextureDrawOptions.Default;

            Elements = new PopUpElementCollection();
            Children = new List<PopUpWindow>();
            Shortcuts = new Dictionary<Keys, PopUpWindowHandler>();
        }
#nullable disable

        /// <summary>Creates a new window which has a size of given <paramref name="width"/> and <paramref name="height"/>.</summary>
        /// <param name="width">Width of window.</param>
        /// <param name="height">Height of window.</param>
        public PopUpWindow(int width, int height) : this(width, height, null)
        {
        }

        /// <summary>Adds the window to top of the screen.</summary>
        public void AddToScreen()
        {
            if (WindowLayers.TryPeek(out PopUpWindow oldPeekWindow) && oldPeekWindow.FreezeUponPeekIsLost) oldPeekWindow.Freeze();

            WindowLayers.Push(this);

            Added(this);
        }

        /// <summary>Removes the window from the top of the screen if it is at the top of the screen.</summary>
        public void RemoveFromScreen()
        {
            if (WindowLayers.TryPeek(out PopUpWindow windowToRemove) && windowToRemove == this)
            {
                WindowLayers.Pop();
                Removed(this);
            }

            if (WindowLayers.TryPeek(out PopUpWindow newPeekWindow) && newPeekWindow.FreezeUponPeekIsLost) newPeekWindow.Unfreeze();
        }

        /// <summary>Freezes the window and its elements with given freezing option. 
        /// Uses <see cref="FreezingOption"/> if <paramref name="option"/> equals to <see cref="FreezingOptions.CurrentOption"/>.</summary>
        /// <param name="option">The option presenting freeze actions that will be applied.</param>
        public void Freeze(FreezingOptions option = FreezingOptions.CurrentOption)
        {
            isFrozen = true;

            if (option != FreezingOptions.CurrentOption) FreezingOption = option;

            foreach (var element in Elements) element.Freeze(FreezingOption);
            BackgroundAnimation?.Stop();

            Frozen(this);
        }

        /// <summary>Unfreezes the window and its elements.</summary>
        public void Unfreeze()
        {
            isFrozen = false;

            foreach (var element in Elements) element.Unfreeze();
            BackgroundAnimation?.Play();

            Unfreezed(this);
        }

        #region Instance Methods
        public void Update()
        {
            foreach (var element in Elements)
            {
                if (element.Visibility == Visibility.Hidden || (element.IsFrozen && element.FreezingOption.HasFlag(FreezingOptions.StopUpdates))) continue;

                element.UpdateElements();
            }

            DrawHelper.SetRenderTarget(windowTexture);
            DrawHelper.BeginBatch(drawOptions);

            // drawing background
            if (BackgroundAnimation != null)
            {
                DrawHelper.Batch.Draw(BackgroundAnimation, new Rectangle(new Point(), bounds.Size), Color.White * this.Opacity);
            }

            // drawing elements
            foreach (var element in Elements)
            {
                if (element.Visibility != Visibility.Visible) continue;

                foreach (var displayInfo in element.GetDisplayInfos())
                {
                    DrawHelper.Batch.Draw(displayInfo.GetTexture(), displayInfo.Boundaries, Color.White * displayInfo.Opacity * this.Opacity);
                }
            }

            DrawHelper.EndBatch();

            foreach (var childWindow in Children)
            {
                if (childWindow.IsFrozen && childWindow.FreezingOption.HasFlag(FreezingOptions.StopUpdates)) continue;

                childWindow.Update();
            }
        }
        
        public void Draw(SpriteBatch batch)
        {
            batch.Draw(windowTexture, new Rectangle(bounds.Location.TimesFloat(GlobalScale), bounds.Size.TimesFloat(Scale * GlobalScale)), Color.White * GlobalOpacity);

            foreach (var childWindow in Children)
            {
                childWindow.Draw(batch);
            }
        }

        private void OnMouseButtonChange(PopUpWindowEventArgs e)
        {
            if (CloseWithSelectingOutside && !Bounds.Contains(e.MousePoint))
            {
                RemoveFromScreen();
                return;
            }

            ISelectable selectedElement = Elements.FindWithType<ISelectable>(element => element.Visibility != Visibility.Hidden &&

                                                                            (element as ISelectable).CanBeSelected(e.MousePoint) &&
                                                                           !(element as ISelectable).DoNotSelectIf(element) &&
                                                                            (element as ISelectable).ButtonsToSelect.HasAnyFlagFrom(e.NewlyPressedButtons));

            selectedElement?.OnSelect(e);

            IEnumerable<ISelectable> outsideSelectables = Elements.WhereWithType<ISelectable>(element => element.Visibility != Visibility.Hidden &&
                                                                                                        (element as ISelectable).EnabledOutsideSelecting);
            foreach (var element in outsideSelectables)
            {
                if (!element.CanBeSelected(e.MousePoint)) element.OnSelectingOutside(e);
            }

            MouseButtonChange(this, e);

            foreach (var childWindow in Children)
            {
                PopUpWindowEventArgs newArgs = e with { MousePoint = (e.MousePoint - childWindow.Bounds.Location).TimesFloat(1 / childWindow.Scale) };
                childWindow.OnMouseButtonChange(newArgs);
            }
        }

        private void OnMousePointChange(PopUpWindowEventArgs e)
        {
            IEnumerable<IHoverable> hoverableElements = Elements.WhereWithType<IHoverable>(element => element.Visibility != Visibility.Hidden &&
                                                                                                    !(element as IHoverable).DoNotHoverIf(element));
            bool endAllHovers = false;
            foreach (var element in hoverableElements)
            {
                if (endAllHovers)
                {
                    element.OnHoverStateChange(e, false);
                    continue;
                }

                if (element.CanBeHovered(e.MousePoint))
                {
                    if (!element.IsHovered) element.OnHoverStateChange(e, true);

                    if (!EnableMultiHover) endAllHovers = true;
                }
                else if (element.IsHovered)
                {
                    element.OnHoverStateChange(e, false);
                }

                if (element.IsHovered) element.OnHovering(e);
            }

            IEnumerable<IHoldable> holdableElements =
                Elements.WhereWithType<IHoldable>(element => element.Visibility != Visibility.Hidden &&
                                                           ((element as IHoldable).CanBeHeld(e.MousePoint) || (element as IHoldable).IsHeld) &&
                                                           !(element as IHoldable).DoNotHoldIf(element));

            IEnumerable<IHoldable> alreadyHeldElements = holdableElements.ToList().Where(element => element.IsHeld);
            foreach (var holdable in alreadyHeldElements)
            {
                holdable.HoldStatusUpdate(e, holdable.ButtonsToHold.HasAnyFlagFrom(e.PressedButtons));

                if (!EnableMultiHold) break;
            }

            if (!alreadyHeldElements.Any() || EnableMultiHold)
            {
                foreach (var element in holdableElements.Except(alreadyHeldElements))
                {
                    if (element.CanBeHeld(e.MousePoint))
                    {
                        element.HoldStatusUpdate(e, element.ButtonsToHold.HasAnyFlagFrom(e.PressedButtons));

                        if (!EnableMultiHold) break;
                    }
                }
            }

            MousePointChange(this, e);

            foreach (var childWindow in Children)
            {
                PopUpWindowEventArgs newArgs = e with { MousePoint = (e.MousePoint - childWindow.Bounds.Location).TimesFloat(1 / childWindow.Scale) };
                childWindow.OnMousePointChange(newArgs);
            }
        }

        private void OnMouseWheelChange(PopUpWindowEventArgs e)
        {
            IMouseWheelReceiver mouseWheelReceiver =
                Elements.FindWithType<IMouseWheelReceiver>(element => element.Visibility != Visibility.Hidden &&
                                                                     (element as IMouseWheelReceiver).CanReceiveWheelChangeInput(e.MousePoint) &&
                                                                    !(element as IMouseWheelReceiver).DoNotReceiveWheelChangeIf(element));
            mouseWheelReceiver?.OnMouseWheelChangeReceive(e);

            MouseWheelChange(this, e);

            foreach (var childWindow in Children)
            {
                PopUpWindowEventArgs newArgs = e with { MousePoint = (e.MousePoint - childWindow.Bounds.Location).TimesFloat(1 / childWindow.Scale) };
                childWindow.OnMouseWheelChange(newArgs);
            }
        }

        private void OnKeyDown(PopUpWindowEventArgs e, Keys key)
        {
            if (Shortcuts.TryGetValue(key, out var func))
            {
                func(this, e);

                if (BlockKeyInputEventsWithShortcuts) return;
            }

            IKeyInputReceiver keyReceiver =
                Elements.FindWithType<IKeyInputReceiver>(element => element.Visibility != Visibility.Hidden &&
                                                                   (element as IKeyInputReceiver).IsInKeyInputMode);
            keyReceiver?.OnKeyInputReceive(e, key);

            KeyDown(this, e, key);

            foreach (var childWindow in Children)
            {
                PopUpWindowEventArgs newArgs = e with { MousePoint = (e.MousePoint - childWindow.Bounds.Location).TimesFloat(1 / childWindow.Scale) };
                childWindow.OnKeyDown(newArgs, key);
            }
        }

        private void OnKeyUp(PopUpWindowEventArgs e, Keys key)
        {
            KeyUp(this, e, key);

            foreach (var childWindow in Children)
            {
                PopUpWindowEventArgs newArgs = e with { MousePoint = (e.MousePoint - childWindow.Bounds.Location).TimesFloat(1 / childWindow.Scale) };
                childWindow.OnKeyUp(newArgs, key);
            }
        }

        private void OnTextInput(PopUpWindowEventArgs e, char ch)
        {
            ITextInputReceiver textReceiver =
                Elements.FindWithType<ITextInputReceiver>(element => element.Visibility != Visibility.Hidden &&
                                                                    (element as ITextInputReceiver).IsInTextInputMode);
            textReceiver?.OnTextInputReceive(e, ch);

            TextInput(this, e, ch);

            foreach (var childWindow in Children)
            {
                PopUpWindowEventArgs newArgs = e with { MousePoint = (e.MousePoint - childWindow.Bounds.Location).TimesFloat(1 / childWindow.Scale) };
                childWindow.OnTextInput(newArgs, ch);
            }
        }

        private void OnFileDrop(PopUpWindowEventArgs e, FileDropEventArgs fileArgs)
        {
            FileDrop(this, e, fileArgs);

            foreach (var childWindow in Children)
            {
                PopUpWindowEventArgs newArgs = e with { MousePoint = (e.MousePoint - childWindow.Bounds.Location).TimesFloat(1 / childWindow.Scale) };
                childWindow.OnFileDrop(newArgs, fileArgs);
            }
        }
        #endregion

        #region Static Methods
        internal static void Initialize()
        {
            if (IsInitialized) return;

            InputManager.MouseButtonPressChange += StaticOnMouseButtonChange;
            InputManager.MousePointChange += StaticOnMousePointChange;
            InputManager.MouseWheelChange += StaticOnMouseWheelChange;
            InputManager.KeyDown += StaticOnKeyDown;
            InputManager.KeyUp += StaticOnKeyUp;
            InputManager.TextInput += StaticOnTextInput;
            InputManager.FileDrop += StaticOnFileDrop;

            IsInitialized = true;
        }

        internal static void UpdateWindows()
        {
            InputManager.Update();

            foreach (var window in WindowLayers)
            {
                if (window.IsFrozen && window.FreezingOption.HasFlag(FreezingOptions.StopUpdates)) continue;

                window.Update();
            }
        }

        internal static void DrawWindows(SpriteBatch batch)
        {
            foreach (var window in WindowLayers.Reverse())
            {
                window.Draw(batch);
            }
        }

        private static void StaticOnMouseButtonChange(PopUpWindowEventArgs e)
        {
            // for ISelectable
            if (WindowLayers.Count == 0) return;

            PopUpWindow peekWindow = WindowLayers.Peek();
            PopUpWindowEventArgs newArgs = e with { MousePoint = (e.MousePoint.TimesFloat(1 / GlobalScale) - peekWindow.Bounds.Location).TimesFloat(1 / peekWindow.Scale) };

            peekWindow.OnMouseButtonChange(newArgs);
        }

        private static void StaticOnMousePointChange(PopUpWindowEventArgs e)
        {
            WindowOptions.InternalExtenderUpdate(e);

            // for IHoverable & IHoldable
            if (WindowLayers.Count == 0) return;

            PopUpWindow peekWindow = WindowLayers.Peek();
            PopUpWindowEventArgs newArgs = e with { MousePoint = (e.MousePoint.TimesFloat(1 / GlobalScale) - peekWindow.Bounds.Location).TimesFloat(1 / peekWindow.Scale) };

            peekWindow.OnMousePointChange(newArgs);
        }

        private static void StaticOnMouseWheelChange(PopUpWindowEventArgs e)
        {
            if (WindowLayers.Count == 0) return;

            PopUpWindow peekWindow = WindowLayers.Peek();
            PopUpWindowEventArgs newArgs = e with { MousePoint = (e.MousePoint.TimesFloat(1 / GlobalScale) - peekWindow.Bounds.Location).TimesFloat(1 / peekWindow.Scale) };

            peekWindow.OnMouseWheelChange(newArgs);
        }

        private static void StaticOnKeyDown(PopUpWindowEventArgs e, Keys key)
        {
            // for IKeyInputReceiver
            if (WindowLayers.Count == 0) return;

            PopUpWindow peekWindow = WindowLayers.Peek();
            PopUpWindowEventArgs newArgs = e with { MousePoint = (e.MousePoint.TimesFloat(1 / GlobalScale) - peekWindow.Bounds.Location).TimesFloat(1 / peekWindow.Scale) };

            peekWindow.OnKeyDown(newArgs, key);
        }

        private static void StaticOnKeyUp(PopUpWindowEventArgs e, Keys key)
        {
            if (WindowLayers.Count == 0) return;

            PopUpWindow peekWindow = WindowLayers.Peek();
            PopUpWindowEventArgs newArgs = e with { MousePoint = (e.MousePoint.TimesFloat(1 / GlobalScale) - peekWindow.Bounds.Location).TimesFloat(1 / peekWindow.Scale) };

            peekWindow.OnKeyUp(newArgs, key);
        }

        private static void StaticOnTextInput(PopUpWindowEventArgs e, char ch)
        {
            // for ITextInputReceiver
            if (WindowLayers.Count == 0) return;

            PopUpWindow peekWindow = WindowLayers.Peek();
            PopUpWindowEventArgs newArgs = e with { MousePoint = (e.MousePoint.TimesFloat(1 / GlobalScale) - peekWindow.Bounds.Location).TimesFloat(1 / peekWindow.Scale) };

            peekWindow.OnTextInput(newArgs, ch);
        }

        private static void StaticOnFileDrop(PopUpWindowEventArgs e, FileDropEventArgs fileArgs)
        {
            if (WindowLayers.Count == 0) return;

            PopUpWindow peekWindow = WindowLayers.Peek();
            PopUpWindowEventArgs newArgs = e with { MousePoint = (e.MousePoint.TimesFloat(1 / GlobalScale) - peekWindow.Bounds.Location).TimesFloat(1 / peekWindow.Scale) };

            peekWindow.OnFileDrop(newArgs, fileArgs);
        }
        #endregion
    }
}
