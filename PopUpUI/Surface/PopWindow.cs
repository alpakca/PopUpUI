using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PopUpUI.Containers;
using PopUpUI.Elements;
using PopUpUI.Input;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 8632

namespace PopUpUI.Surface
{
    public class PopWindow
    {
        private Rectangle _boundaries;
        private RenderTarget2D _windowTexture;

        private readonly TextureDrawOptions _drawOptions;

        /// <summary>Enables more than one <see cref="IHoldable"/> instances to be updated at the same time. Default value is <see langword="true"/>.</summary>
        public bool EnableMultiHold { get; set; } = true;
        /// <summary>Enables more than one <see cref="IHoverable"/> instances to be updated at the same time. Default value is <see langword="true"/>.</summary>
        public bool EnableMultiHover { get; set; } = true;
        /// <summary>The opacity of window. Expected inputs are between 0 and 1. Zero makes it invisible and one makes images' look original.</summary>
        public float Opacity { get; set; } = 1;
        /// <summary>Gets or sets the scale to use when drawing this window to screen. It doesn't effect bounds or texture of <see cref="WindowTexture"/>.</summary>
        public float Scale { get; set; } = 1;

        public Texture2D WindowTexture => _windowTexture;
        public PopElementCollection Elements { get; }

        public Rectangle Boundaries 
        {
            get => _boundaries;
            set
            {
                if (value.Width <= 0 || value.Height <= 0)
                    throw new ArgumentException("Window width and height must be greated than zero");

                if (_boundaries != value)
                {
                    _windowTexture?.Dispose();
                    _windowTexture = DrawHelper.CreateRenderTarget(value.Width, value.Height);

                    _boundaries = value;
                }
            }
        }

        public PopWindow(Point size) : this(size, null)
        {
        }

        public PopWindow(Point size, TextureDrawOptions? drawOptions)
        {
            _drawOptions = drawOptions ?? TextureDrawOptions.Default;

            Boundaries = new Rectangle(new Point(), size);
            Elements = new PopElementCollection();
        }

        internal void FrameUpdate()
        {
            foreach (var element in Elements)
            {
                if (element.Visibility == Visibility.Hidden)
                {
                    continue;
                }

                element.FrameUpdate();
            }
        }

        internal void RenderUpdate()
        {
            foreach (var element in Elements)
            {
                element.RenderUpdate();
            }

            DrawHelper.SetBeginBatch(_windowTexture, _drawOptions);

            foreach (var element in Elements)
            {
                if (element.Visibility != Visibility.Visible)
                {
                    continue;
                }

                foreach (var drawInfo in element.GetDrawInfos())
                {
                    DrawHelper.Batch.Draw(drawInfo.Texture, drawInfo.DestinationArea, drawInfo.Opacity * Color.White);
                }
            }

            DrawHelper.EndBatch();
        }

        internal void OnMousePositionChange(PopUpWindowEventArgs e)
        {
            bool endAllHovers = false;
            foreach (var element in Elements.WhereWithType<IHoverable>(i => true))
            {
                if (endAllHovers)
                {
                    element.OnHoverStateChange(e, false);
                    continue;
                }

                if (element.CanBeHovered(e.MousePoint))
                {
                    if (!element.HoverablePacket.IsHovered) element.OnHoverStateChange(e, true);

                    if (!EnableMultiHover) endAllHovers = true;
                }
                else if (element.HoverablePacket.IsHovered)
                {
                    element.OnHoverStateChange(e, false);
                }

                if (element.HoverablePacket.IsHovered) element.OnHovering(e);
            }

            IEnumerable<IHoldable> holdableElements =
                Elements.WhereWithType<IHoldable>(element => element.CanBeHeld(e.MousePoint) || element.HoldPacket.IsHeld);

            IEnumerable<IHoldable> alreadyHeldElements = holdableElements.ToList().Where(element => element.HoldPacket.IsHeld);
            foreach (var holdable in alreadyHeldElements)
            {
                holdable.HoldStatusUpdate(e, holdable.HoldPacket.ButtonsToHold.HasAnyFlagFrom(e.PressedButtons));

                if (!EnableMultiHold) break;
            }

            if (!alreadyHeldElements.Any() || EnableMultiHold)
            {
                foreach (var element in holdableElements.Except(alreadyHeldElements))
                {
                    if (element.CanBeHeld(e.MousePoint))
                    {
                        element.HoldStatusUpdate(e, element.HoldPacket.ButtonsToHold.HasAnyFlagFrom(e.PressedButtons));

                        if (!EnableMultiHold) break;
                    }
                }
            }
        }

        internal void OnMouseButtonStateChange(PopUpWindowEventArgs e)
        {
            ISelectable selectedElement = Elements.FindWithType<ISelectable>(element => element.CanBeSelected(e.MousePoint) &&
                                                                                        element.SelectablePacket.ButtonsToSelect.HasAnyFlagFrom(e.NewlyPressedButtons));

            selectedElement?.OnSelect(e);

            IEnumerable<ISelectable> outsideSelectables = Elements.WhereWithType<ISelectable>(element => element.SelectablePacket.EnabledOutsideSelecting);
            foreach (var element in outsideSelectables)
            {
                if (!element.CanBeSelected(e.MousePoint)) element.OnSelectingOutside(e);
            }
        }

        internal void OnMouseWheelChange(PopUpWindowEventArgs e)
        {
            IMouseWheelReceiver mouseWheelReceiver = Elements.FindWithType<IMouseWheelReceiver>(element => element.CanReceiveWheelChangeInput(e.MousePoint));

            mouseWheelReceiver?.OnMouseWheelChangeReceive(e);
        }

        internal void OnKeyDown(PopUpWindowEventArgs e, Keys key)
        {
            IKeyReceiver keyReceiver = Elements.FindWithType<IKeyReceiver>(element => element.KeyReceiverPacket.ReceivesKeyInput);

            keyReceiver?.OnKeyDown(e, key);
        }

        internal void OnKeyUp(PopUpWindowEventArgs e, Keys key)
        {
            IKeyReceiver keyReceiver = Elements.FindWithType<IKeyReceiver>(element => element.KeyReceiverPacket.ReceivesKeyInput);

            keyReceiver?.OnKeyUp(e, key);
        }

        internal void OnTextInput(PopUpWindowEventArgs e, char ch)
        {
            ITextReceiver textReceiver = Elements.FindWithType<ITextReceiver>(element => element.TextReceiverPacket.ReceivesTextInput);

            textReceiver?.OnTextInputReceive(e, ch);
        }

        internal void OnFileDrop(PopUpWindowEventArgs e, FileDropEventArgs fileArgs)
        {
            IFileReceiver fileReceiver = Elements.FindWithType<IFileReceiver>(element => element.FilePacket.ReceivesFileDrop);

            fileReceiver?.OnFileDrop(e, fileArgs);
        }
    }
}
