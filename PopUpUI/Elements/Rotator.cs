using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PopUpUI.Input;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;

namespace PopUpUI.Elements
{
    /// <summary>Provides a covering for <see cref="PopUpElement"/> and its events, while it can be rotated.</summary>
    /// <remarks>This instance is recommend to use for changing values (Bounds, Opacity, events etc) instead of original element.
    /// Otherwise changes made may not applied. (e.g. change in Bounds in the original element will not affect rotated element's bounds, it will not be updated.)</remarks>
    public class Rotator : PopUpElement, IHoverable, IHoldable, ISelectable, IKeyInputReceiver, IMouseWheelReceiver, ITextInputReceiver, IDisposable
    {
        private float currentAngle = 0;
        private Rectangle area;
        private RenderTarget2D elementTexture;
        private RenderTarget2D rotatedTexture;

        private readonly PopUpElement elementToRotate;
        private readonly TextureDisplayInfo uiElement;
        private readonly BatchDrawOptions drawOptions;

        public float RotatedAngle => currentAngle;
        public bool IsHovered => elementToRotate is IHoverable hoverable && hoverable.IsHovered;
        public bool IsHeld => elementToRotate is IHoldable holdable && holdable.IsHeld;
        public bool EnabledOutsideSelecting
        {
            get => elementToRotate is ISelectable selectable && selectable.EnabledOutsideSelecting;
            set { if (elementToRotate is ISelectable selectable) selectable.EnabledOutsideSelecting = value; }
        }
        public MouseButtons ButtonsToSelect
        {
            get => elementToRotate is ISelectable selectable ? selectable.ButtonsToSelect : MouseButtons.Left;
            set { if (elementToRotate is ISelectable selectable) selectable.ButtonsToSelect = value; }
        }
        public MouseButtons ButtonsToHold
        {
            get => elementToRotate is IHoldable holdable ? holdable.ButtonsToHold : MouseButtons.Left;
            set { if (elementToRotate is IHoldable holdable) holdable.ButtonsToHold = value; }
        }
        public bool IsInKeyInputMode => elementToRotate is IKeyInputReceiver receiver && receiver.IsInKeyInputMode;
        public bool IsInTextInputMode => elementToRotate is ITextInputReceiver receiver && receiver.IsInTextInputMode;
        public override float Opacity
        {
            get => elementToRotate.Opacity;
            set => elementToRotate.Opacity = value;
        }
        public override Rectangle Area
        {
            get => area;
            set 
            {
                if (value.Size != this.area.Size)
                {
                    elementTexture.Dispose();
                    rotatedTexture.Dispose();

                    elementTexture = DrawHelper.CreateRenderTarget(value.Width, value.Height);
                    int diameter = (int)Math.Sqrt(Math.Pow(value.Width, 2) + Math.Pow(value.Height, 2));
                    rotatedTexture = DrawHelper.CreateRenderTarget(diameter, diameter);

                    uiElement.Texture = rotatedTexture;
                }

                uiElement.Location = value.Location + new Point(-(rotatedTexture.Width - value.Width) / 2,
                                                                       -(rotatedTexture.Height - value.Height) / 2);
                elementToRotate.Area = value;
                area = value;
                RotateBy(currentAngle);
            }
        }

        public Predicate<PopUpElement> DoNotHoverIf
        {
            get => (elementToRotate as IHoverable)?.DoNotHoverIf ?? PopUpElement.DoNotActionIfPredicate;
            set { if (elementToRotate is IHoverable hoverable) hoverable.DoNotHoverIf = value; }
        }
        public Predicate<PopUpElement> DoNotHoldIf
        {
            get => (elementToRotate as IHoldable)?.DoNotHoldIf ?? PopUpElement.DoNotActionIfPredicate;
            set { if (elementToRotate is IHoldable holdable) holdable.DoNotHoldIf = value; }
        }
        public Predicate<PopUpElement> DoNotSelectIf
        {
            get => (elementToRotate as ISelectable)?.DoNotSelectIf ?? PopUpElement.DoNotActionIfPredicate;
            set { if (elementToRotate is ISelectable selectable) selectable.DoNotSelectIf = value; }
        }
        public Predicate<PopUpElement> FinishKeyInputModeIf
        {
            get => (elementToRotate as IKeyInputReceiver)?.FinishKeyInputModeIf ?? PopUpElement.DoNotActionIfPredicate;
            set { if (elementToRotate is IKeyInputReceiver receiver) receiver.FinishKeyInputModeIf = value; }
        }
        public Predicate<PopUpElement> DoNotReceiveWheelChangeIf
        {
            get => (elementToRotate as IMouseWheelReceiver)?.DoNotReceiveWheelChangeIf ?? PopUpElement.DoNotActionIfPredicate;
            set { if (elementToRotate is IMouseWheelReceiver receiver) receiver.DoNotReceiveWheelChangeIf = value; }
        }
        public Predicate<PopUpElement> FinishTextInputModeIf
        {
            get => (elementToRotate as ITextInputReceiver)?.FinishTextInputModeIf ?? PopUpElement.DoNotActionIfPredicate;
            set { if (elementToRotate is ITextInputReceiver receiver) receiver.FinishTextInputModeIf = value; }
        }

        public event PopUpElementHandler Hovering
        {
            add { if (elementToRotate is IHoverable hoverable) hoverable.Hovering += value; }
            remove { if (elementToRotate is IHoverable hoverable) hoverable.Hovering -= value; }
        }
        public event PopUpElementHandler<bool> HoverStateChange
        {
            add { if (elementToRotate is IHoverable hoverable) hoverable.HoverStateChange += value; }
            remove { if (elementToRotate is IHoverable hoverable) hoverable.HoverStateChange -= value; }
        }
        public event PopUpElementHandler Held
        {
            add { if (elementToRotate is IHoldable holdable) holdable.Held += value; }
            remove { if (elementToRotate is IHoldable holdable) holdable.Held -= value; }
        }
        public event PopUpElementHandler<bool> HoldStateChange
        {
            add { if (elementToRotate is IHoldable holdable) holdable.HoldStateChange += value; }
            remove { if (elementToRotate is IHoldable holdable) holdable.HoldStateChange -= value; }
        }
        public event PopUpElementHandler Selected
        {
            add { if (elementToRotate is ISelectable selectable) selectable.Selected += value; }
            remove { if (elementToRotate is ISelectable selectable) selectable.Selected -= value; }
        }
        public event PopUpElementHandler OutsideSelected
        {
            add { if (elementToRotate is ISelectable selectable) selectable.OutsideSelected += value; }
            remove { if (elementToRotate is ISelectable selectable) selectable.OutsideSelected -= value; }
        }
        public event PopUpElementHandler<Keys> KeyInputReceived
        {
            add { if (elementToRotate is IKeyInputReceiver receiver) receiver.KeyInputReceived += value; }
            remove { if (elementToRotate is IKeyInputReceiver receiver) receiver.KeyInputReceived -= value; }
        }
        public event PopUpElementHandler MouseWheelChanged
        {
            add { if (elementToRotate is IMouseWheelReceiver receiver) receiver.MouseWheelChanged += value; }
            remove { if (elementToRotate is IMouseWheelReceiver receiver) receiver.MouseWheelChanged -= value; }
        }
        public event PopUpElementHandler<char> TextInputReceived
        {
            add { if (elementToRotate is ITextInputReceiver receiver) receiver.TextInputReceived += value; }
            remove { if (elementToRotate is ITextInputReceiver receiver) receiver.TextInputReceived -= value; }
        }

        /// <summary>Initializes a new instance of <see cref="Rotator"/> class with given element and optional bounds for the element.</summary>
        /// <param name="element">The element that will be rotated and event will be called on.</param>
        /// <param name="bounds">The bounds of the element. If element's bounds are not specified, they must be specified here, else an exception will be thrown.</param>
        /// <param name="options">Options to control drawing behaviour.</param>
        /// <exception cref="ArgumentException"></exception>
        public Rotator(PopUpElement element, BatchDrawOptions? options)
        {
            ArgumentNullException.ThrowIfNull(element);
            if (element.Area == Rectangle.Empty) throw new ArgumentException();

            elementToRotate = element;
            this.area = element.Area;
            drawOptions = options ?? BatchDrawOptions.Default;

            elementTexture = DrawHelper.CreateRenderTarget(this.area.Width, this.area.Height);
            int diameter = (int)Math.Sqrt(Math.Pow(this.area.Width, 2) + Math.Pow(this.area.Height, 2));
            rotatedTexture = DrawHelper.CreateRenderTarget(diameter, diameter);

            RotateBy(0);
            uiElement = new TextureDisplayInfo { Texture = rotatedTexture, Location = area.Location + new Point(-(rotatedTexture.Width - this.area.Width) / 2,
                                                                                                                -(rotatedTexture.Height - this.area.Height) / 2) };
        }

        public Rotator(PopUpElement element) : this(element, null)
        {
        }

        ~Rotator()
        {
            Dispose();
        }

        public void Dispose()
        {
            elementTexture.Dispose();
            rotatedTexture.Dispose();
            GC.SuppressFinalize(this);
        }

        public override IEnumerable<IUIDisplayInfo> GetDisplayInfos()
        {
            return [uiElement];
        }

        protected override void ToggleAnimationStates(bool willPlay)
        {
            
        }

        public override void UpdateElements()
        {
            elementToRotate.UpdateElements();

            RotateBy(currentAngle);
        }

        public override bool IsInside(Point point)
        {
            return elementToRotate.IsInside(GetRotatedPoint(point));
        }

        /// <summary>Rotates the element by the given degree.</summary>
        /// <param name="degree">The angle to rotate the element.</param>
        public void RotateBy(int degree)
        {
            DrawHelper.SetRenderTarget(elementTexture);
            DrawHelper.BeginBatch(drawOptions);

            foreach (var uiElement in elementToRotate.GetDisplayInfos())
            {
                DrawHelper.Batch.Draw(uiElement.GetTexture(), new Rectangle(uiElement.Location - this.area.Location, uiElement.Size), Color.White * uiElement.Opacity);
            }

            DrawHelper.EndBatch();


            DrawHelper.SetRenderTarget(rotatedTexture);
            DrawHelper.BeginBatch(drawOptions);

            DrawHelper.Batch.Draw(elementTexture, new Rectangle(rotatedTexture.Bounds.Size / new Point(2), elementTexture.Bounds.Size), null,
                       Color.White, MathHelper.ToRadians(degree),
                       elementTexture.Bounds.Size.ToVector2() / new Vector2(2), SpriteEffects.None, 0f);

            DrawHelper.EndBatch();

            currentAngle = degree;
        }

        public void RotateBy(float radians)
        {
            DrawHelper.SetRenderTarget(elementTexture);
            DrawHelper.BeginBatch(drawOptions);

            foreach (var uiElement in elementToRotate.GetDisplayInfos())
            {
                DrawHelper.Batch.Draw(uiElement.GetTexture(), new Rectangle(uiElement.Location - this.area.Location, uiElement.Size), Color.White * uiElement.Opacity);
            }

            DrawHelper.EndBatch();


            DrawHelper.SetRenderTarget(rotatedTexture);
            DrawHelper.BeginBatch(drawOptions);

            DrawHelper.Batch.Draw(elementTexture, new Rectangle(rotatedTexture.Bounds.Size / new Point(2), elementTexture.Bounds.Size), null,
                       Color.White, radians,
                       elementTexture.Bounds.Size.ToVector2() / new Vector2(2), SpriteEffects.None, 0f);

            DrawHelper.EndBatch();

            currentAngle = radians;
        }

        // rotates the "original" in the opposite direction to which the element is rotated
        private Point GetRotatedPoint(Point original)
        {
            original -= elementToRotate.Area.Size / new Point(2) + elementToRotate.Area.Location;
            Point transformed = new Point(original.X, -original.Y);

            double radius = Math.Sqrt(Math.Pow(transformed.X, 2) + Math.Pow(transformed.Y, 2));
            double betaAngle = Math.Atan((transformed.X - transformed.Y * Math.Tan(MathHelper.ToRadians(currentAngle))) / 
                                         (transformed.Y + transformed.X * Math.Tan(MathHelper.ToRadians(currentAngle))));

            int newXPoint = (int)(radius * Math.Sin(betaAngle));
            int newYPoint = (int)(radius * Math.Cos(betaAngle));

            return new Point(-newXPoint, newYPoint) + elementToRotate.Area.Size / new Point(2) + elementToRotate.Area.Location;
        }

        public bool CanBeHeld(Point point)
        {
            if (elementToRotate is IHoldable holdable)
            {
                return holdable.CanBeHeld(GetRotatedPoint(point));
            }

            return false;
        }

        public bool CanBeHovered(Point point)
        {
            if (elementToRotate is IHoverable hoverable)
            {
                return hoverable.CanBeHovered(GetRotatedPoint(point));
            }

            return false;
        }

        public bool CanBeSelected(Point point)
        {
            if (elementToRotate is ISelectable selectedable)
            {
                return selectedable.CanBeSelected(GetRotatedPoint(point));
            }

            return false;
        }

        public bool CanReceiveWheelChangeInput(Point point)
        {
            if (elementToRotate is IMouseWheelReceiver receiver)
            {
                return receiver.CanReceiveWheelChangeInput(GetRotatedPoint(point));
            }

            return false;
        }

        void IHoldable.HoldStatusUpdate(PopUpWindowEventArgs e, bool isHeld)
        {
            if (elementToRotate is IHoldable holdable) holdable.HoldStatusUpdate(e, isHeld);
        }

        void IHoverable.OnHovering(PopUpWindowEventArgs e)
        {
            if (elementToRotate is IHoverable hoverable) hoverable.OnHovering(e);
        }

        void IHoverable.OnHoverStateChange(PopUpWindowEventArgs e, bool isHovered)
        {
            if (elementToRotate is IHoverable hoverable) hoverable.OnHoverStateChange(e, isHovered);
        }

        void ISelectable.OnSelect(PopUpWindowEventArgs e)
        {
            if (elementToRotate is ISelectable selectable) selectable.OnSelect(e);
        }

        void ISelectable.OnSelectingOutside(PopUpWindowEventArgs e)
        {
            if (elementToRotate is ISelectable selectable) selectable.OnSelectingOutside(e);
        }

        void IKeyInputReceiver.OnKeyInputReceive(PopUpWindowEventArgs e, Keys key)
        {
            if (elementToRotate is IKeyInputReceiver receiver) receiver.OnKeyInputReceive(e, key);
        }

        void IMouseWheelReceiver.OnMouseWheelChangeReceive(PopUpWindowEventArgs e)
        {
            if (elementToRotate is IMouseWheelReceiver receiver) receiver.OnMouseWheelChangeReceive(e);
        }

        void ITextInputReceiver.OnTextInputReceive(PopUpWindowEventArgs e, char ch)
        {
            if (elementToRotate is ITextInputReceiver receiver) receiver.OnTextInputReceive(e, ch);
        }
    }
}
