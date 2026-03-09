using Microsoft.Xna.Framework;
using PopUpUI.Elements;
using PopUpUI.Input;
using System;

namespace PopUpUI.NewElements
{
    internal class Movable : PopElement, IHoldable
    {
        protected bool isHeld;
        protected Point lastMousePosition;

        public bool IsHeld => isHeld;
        public MovementDirection MovementDirection { get; }

        public IHoldablePacket HoldPacket { get; set; }

        public Movable(MovementDirection movementDirection)
        {
            MovementDirection = movementDirection;
        }

        public virtual void OnHolding(PopUpWindowEventArgs e)
        {
            UpdatePosition(e.MousePoint);

            HoldPacket?.RaiseHeld(this, e);
        }

        public virtual void OnHoldStateChange(PopUpWindowEventArgs e, bool startedHolding)
        {
            if (startedHolding)
            {
                lastMousePosition = e.MousePoint;
            }

            this.isHeld = startedHolding;
            HoldPacket?.RaiseHoldStateChange(this, e, startedHolding);
        }

        protected virtual void OnPositionChange()
        {
        }

        public bool CanBeHeld(Point point)
        {
            return _boundaries.Contains(point);
        }

        private void UpdatePosition(Point mousePosition)
        {
            int widthChange = mousePosition.X - lastMousePosition.X;
            int heightChange = mousePosition.Y - lastMousePosition.Y;

            Point newLocation = Boundaries.Location + MovementDirection switch
            {
                MovementDirection.Any => new Point(widthChange, heightChange),
                MovementDirection.Horizontal => new Point(widthChange, 0),
                MovementDirection.Vertical => new Point(0, heightChange),

                _ => throw new Exception()
            };

            lastMousePosition = mousePosition;
            _boundaries.Location = newLocation;

            if (newLocation != Point.Zero)
            {
                OnPositionChange();
            }
        }
    }
}
