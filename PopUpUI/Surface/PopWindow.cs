using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PopUpUI.Containers;
using PopUpUI.Elements;
using PopUpUI.Input;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#pragma warning disable 8632

namespace PopUpUI.Surface
{
    public class PopWindow : IDisposable
    {
        private Rectangle _boundaries;
        private RenderTarget2D _windowTexture;

        /// <summary>Enables more than one <see cref="IHoldable"/> instances to be updated at the same time. Default value is <see langword="true"/>.</summary>
        public bool EnableMultiHold { get; set; } = true;
        /// <summary>Enables more than one <see cref="IHoverable"/> instances to be updated at the same time. Default value is <see langword="true"/>.</summary>
        public bool EnableMultiHover { get; set; } = true;
        /// <summary>The opacity of window. Expected inputs are between 0 and 1. Zero makes it invisible and one makes images' look original.</summary>
        public float Opacity { get; set; } = 1;
        /// <summary>Gets or sets the scale to use when drawing this window to screen. It doesn't effect bounds or texture of <see cref="WindowTexture"/>.</summary>
        public float Scale { get; set; } = 1;

        public RenderTarget2D WindowTexture => _windowTexture;

        public BatchDrawOptions DrawOptions { get; }
        public PopElementCollection Elements { get; }

        public Rectangle Boundaries
        {
            get => _boundaries;
            set
            {
                if (value.Width <= 0 || value.Height <= 0)
                    throw new ArgumentException("Window width and height must be greated than zero");

                if (_boundaries.Size != value.Size)
                {
                    _windowTexture?.Dispose();
                    _windowTexture = DrawHelper.CreateRenderTarget(value.Width, value.Height);
                }

                _boundaries = value;
            }
        }

        public PopWindow(Point size) : this(size, null)
        {
        }

        public PopWindow(Point size, BatchDrawOptions? drawOptions)
        {
            DrawOptions = drawOptions ?? BatchDrawOptions.Default;

            Boundaries = new Rectangle(new Point(), size);
            Elements = new PopElementCollection();
        }

        ~PopWindow()
        {
            Dispose();
        }

        public virtual Point TransformMousePoint(Point mousePoint) => mousePoint;

        public virtual void Dispose()
        {
            _windowTexture?.Dispose();
            GC.SuppressFinalize(this);
        }

        protected internal virtual void Update()
        {
            foreach (var element in Elements)
            {
                if (element.Visibility == Visibility.Hidden)
                {
                    continue;
                }

                element.Update();
            }
        }

        protected internal virtual void RenderUpdate()
        {
            foreach (var element in Elements)
            {
                element.RenderUpdate();
            }

            DrawHelper.SetBeginBatch(_windowTexture, DrawOptions);

            foreach (var element in Elements)
            {
                if (element.Visibility != Visibility.Visible)
                {
                    continue;
                }

                foreach (var drawInfo in element.GetDrawInfos())
                {
                    // only render the texture if it can be seen
                    if (IsDestinationVisible(drawInfo.DestinationArea))
                    {
                        DrawHelper.Batch.Draw(drawInfo.GetTexture(), drawInfo.DestinationArea, drawInfo.Opacity * Color.White);
                    }
                }
            }

            DrawHelper.EndBatch();
        }

        protected bool IsDestinationVisible(Rectangle destination)
        {
            return destination.Bottom > 0 || destination.Right > 0 || destination.Top < _boundaries.Height || destination.Left < _boundaries.Width;
        }

        protected internal virtual void OnMousePositionChange(Input.PopWindowMouseEventArgs e)
        {
            HandleHoverableElementEvents(e);
            HandleHoldableElementEvents(e);
        }

        private void HandleHoverableElementEvents(Input.PopWindowMouseEventArgs e)
        {
            bool endAllHovers = false;
            foreach (var element in Elements.WhereWithType<IHoverable>())
            {
                if (endAllHovers)
                {
                    element.OnHoverStateChange(e, false);
                    continue;
                }

                if (element.CanBeHovered(e.MousePosition))
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
        }

        private void HandleHoldableElementEvents(Input.PopWindowMouseEventArgs e)
        {
            if (!EnableMultiHold)
            {
                IHoldable? elementToHold = null;
                IEnumerable<IHoldable> holdableElements = Elements.WhereWithType<IHoldable>();

                foreach (var element in holdableElements)
                {
                    if (element.IsHeld && HoldableShouldBeHeld(element, e))
                    {
                        elementToHold = element;
                        break;
                    }
                }

                if (elementToHold == null)
                {
                    foreach (var element in holdableElements)
                    {
                        if (HoldableShouldBeHeld(element, e))
                        {
                            elementToHold = element;
                            break;
                        }
                    }
                }

                foreach (var element in holdableElements)
                {
                    bool shouldBeHeld = element == elementToHold;

                    if (shouldBeHeld != element.IsHeld)
                    {
                        element.OnHoldStateChange(e, shouldBeHeld);
                    }

                    if (shouldBeHeld)
                    {
                        element.OnHolding(e);
                    }
                }
            }
            else
            {
                foreach (var element in Elements.WhereWithType<IHoldable>())
                {
                    bool shouldBeHeld = HoldableShouldBeHeld(element, e);

                    if (shouldBeHeld != element.IsHeld)
                    {
                        element.OnHoldStateChange(e, shouldBeHeld);
                    }

                    if (element.IsHeld)
                    {
                        element.OnHolding(e);
                    }
                }
            }

            static bool HoldableShouldBeHeld(IHoldable element, Input.PopWindowMouseEventArgs e)
            {
                return (element.CanBeHeld(e.MousePosition) || element.IsHeld) && e.CurrentButtons.OverlapsWith(element.HoldPacket.ButtonsToHold);
            }
        }

        protected internal virtual void OnMouseButtonStateChange(Input.PopWindowMouseEventArgs e)
        {
            ISelectable? selectedElement = Elements.FindWithType<ISelectable>(element => element.CanBeSelected(e.MousePosition) &&
                                                                                         e.FirstTimeButtons.OverlapsWith(element.SelectablePacket.ButtonsToSelect));

            selectedElement?.OnSelect(e);

            IEnumerable<ISelectable> outsideSelectables = Elements.WhereWithType<ISelectable>(element => element.SelectablePacket.EnabledOutsideSelecting);
            foreach (var element in outsideSelectables)
            {
                if (!element.CanBeSelected(e.MousePosition)) element.OnSelectingOutside(e);
            }

            HandleHoldableElementEvents(e);
        }

        protected internal virtual void OnMouseVerticalScrollChange(Input.PopWindowMouseEventArgs e, int change)
        {
            IMouseWheelReceiver? mouseWheelReceiver = Elements.FindWithType<IMouseWheelReceiver>(element => element.CanReceiveWheelChangeInput(e.MousePosition));

            mouseWheelReceiver?.OnMouseVerticalScrollChangeReceive(e, change);
        }

        protected internal virtual void OnMouseHorizontalScrollChange(Input.PopWindowMouseEventArgs e, int change)
        {
            IMouseWheelReceiver? mouseWheelReceiver = Elements.FindWithType<IMouseWheelReceiver>(element => element.CanReceiveWheelChangeInput(e.MousePosition));

            mouseWheelReceiver?.OnMouseHorizontalScrollChangeReceive(e, change);
        }

        protected internal virtual void OnKeyDown(Input.PopWindowMouseEventArgs e, Keys key)
        {
            IKeyReceiver? keyReceiver = Elements.FindWithType<IKeyReceiver>(element => element.ReceivesKeyInput);

            keyReceiver?.OnKeyDown(e, key);
        }

        protected internal virtual void OnKeyUp(Input.PopWindowMouseEventArgs e, Keys key)
        {
            IKeyReceiver? keyReceiver = Elements.FindWithType<IKeyReceiver>(element => element.ReceivesKeyInput);

            keyReceiver?.OnKeyUp(e, key);
        }

        protected internal virtual void OnTextInput(Input.PopWindowMouseEventArgs e, char ch)
        {
            ITextReceiver? textReceiver = Elements.FindWithType<ITextReceiver>(element => element.ReceivesTextInput);

            textReceiver?.OnTextInputReceive(e, ch);
        }

        protected internal virtual void OnFileDrop(Input.PopWindowMouseEventArgs e, FileDropEventArgs fileArgs)
        {
            IFileReceiver? fileReceiver = Elements.FindWithType<IFileReceiver>(element => element.CanReceiveFileDrop(e.MousePosition));

            fileReceiver?.OnFileDrop(e, fileArgs);
        }
    }
}
